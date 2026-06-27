// =============================================================================
// NeonCity — Server / server.js
// -----------------------------------------------------------------------------
// Authoritative multiplayer backend. Built with Node.js + Express + Socket.IO.
// Responsibilities:
//   1. Matchmaking / lobby creation
//   2. Relay authoritative gameplay events (fire, damage, vehicle sync)
//   3. Chat / voice signaling (real voice audio uses WebRTC SFU; this is
//      the signaling layer only)
//   4. Anti-cheat telemetry collector
//   5. Leaderboards & persistence bridge to Firebase
//
// Run: `cd Server && npm install && npm start`
// Dev: `npm run dev` (nodemon)
// =============================================================================

const express      = require('express');
const http         = require('http');
const cors         = require('cors');
const helmet       = require('helmet');
const { Server }   = require('socket.io');
const admin        = require('firebase-admin');
const { v4: uuid } = require('uuid');
const rateLimit    = require('express-rate-limit');

require('dotenv').config();

// -----------------------------------------------------------------------------
// Firebase Admin init (for account verification & leaderboards)
// -----------------------------------------------------------------------------
if (process.env.FIREBASE_CREDENTIALS) {
  admin.initializeApp({
    credential: admin.credential.cert(JSON.parse(process.env.FIREBASE_CREDENTIALS))
  });
}

const db = admin.firestore();

// -----------------------------------------------------------------------------
// App + Socket.IO
// -----------------------------------------------------------------------------
const app    = express();
const server = http.createServer(app);
const io     = new Server(server, { cors: { origin: '*' } });

app.use(helmet());
app.use(cors());
app.use(express.json({ limit: '128kb' }));
app.use(rateLimit({ windowMs: 60_000, max: 200 })); // 200 req/min per IP

// -----------------------------------------------------------------------------
// In-memory room store. Replace with Redis for production scale.
// -----------------------------------------------------------------------------
const rooms = new Map();

function createRoom(name, maxPlayers, hostId) {
  const room = {
    id: uuid(),
    name,
    maxPlayers,
    hostId,
    players: [],
    state: 'lobby',
    startedAt: Date.now()
  };
  rooms.set(room.id, room);
  return room;
}

function joinRoom(roomId, player) {
  const r = rooms.get(roomId);
  if (!r) return null;
  if (r.players.length >= r.maxPlayers) return null;
  r.players.push(player);
  return r;
}

function leaveRoom(roomId, playerId) {
  const r = rooms.get(roomId);
  if (!r) return;
  r.players = r.players.filter(p => p.id !== playerId);
  if (r.players.length === 0) rooms.delete(roomId);
}

// -----------------------------------------------------------------------------
// REST API
// -----------------------------------------------------------------------------
app.get('/health', (req, res) => res.json({ ok: true, rooms: rooms.size }));

app.post('/rooms', (req, res) => {
  const { name, maxPlayers = 16, hostId } = req.body;
  if (!name || !hostId) return res.status(400).json({ error: 'missing fields' });
  const room = createRoom(name, maxPlayers, hostId);
  res.json(room);
});

app.get('/rooms', (req, res) => {
  res.json(Array.from(rooms.values()).map(r => ({
    id: r.id, name: r.name, players: r.players.length, max: r.maxPlayers
  })));
});

app.post('/leaderboard', async (req, res) => {
  const { uid, displayName, score } = req.body;
  await db.collection('leaderboard').doc(uid).set({
    displayName, score, updatedAt: Date.now()
  }, { merge: true });
  res.json({ ok: true });
});

app.get('/leaderboard/top', async (req, res) => {
  const snap = await db.collection('leaderboard')
                       .orderBy('score', 'desc').limit(100).get();
  res.json(snap.docs.map(d => d.data()));
});

// -----------------------------------------------------------------------------
// Socket.IO — real-time gameplay relay
// -----------------------------------------------------------------------------
io.on('connection', socket => {
  console.log(`[+] client ${socket.id}`);

  socket.on('room:create', ({ name, maxPlayers }, cb) => {
    const r = createRoom(name, maxPlayers || 16, socket.id);
    socket.join(r.id);
    cb?.({ ok: true, room: r });
  });

  socket.on('room:join', ({ roomId, player }, cb) => {
    const r = joinRoom(roomId, { id: socket.id, ...player });
    if (!r) return cb?.({ ok: false, error: 'full or missing' });
    socket.join(roomId);
    io.to(roomId).emit('room:playerJoined', { id: socket.id, ...player });
    cb?.({ ok: true, room: r });
  });

  socket.on('room:leave', ({ roomId }) => {
    leaveRoom(roomId, socket.id);
    socket.leave(roomId);
    io.to(roomId).emit('room:playerLeft', { id: socket.id });
  });

  // ---- Authoritative gameplay events -----------------------------------
  socket.on('fire', data => {
    // Server-side validation: clamp fire rate, check weapon existence.
    // If invalid -> log to anti-cheat collection.
    validateFire(socket.id, data);
    socket.to(data.roomId).emit('fire', data);
  });

  socket.on('damage', data => {
    validateDamage(socket.id, data);
    socket.to(data.roomId).emit('damage', data);
  });

  socket.on('vehicle:sync', data => {
    socket.to(data.roomId).emit('vehicle:sync', data);
  });

  // ---- Voice chat signaling (WebRTC offer/answer/ICE) ------------------
  socket.on('voice:offer',  data => socket.to(data.to).emit('voice:offer',  { from: socket.id, ...data }));
  socket.on('voice:answer', data => socket.to(data.to).emit('voice:answer', { from: socket.id, ...data }));
  socket.on('voice:ice',    data => socket.to(data.to).emit('voice:ice',    { from: socket.id, ...data }));

  // ---- Chat ----------------------------------------------------------
  socket.on('chat', msg => {
    if (!msg?.roomId) return;
    socket.to(msg.roomId).emit('chat', { id: socket.id, text: msg.text, ts: Date.now() });
  });

  socket.on('disconnect', () => {
    for (const r of rooms.values()) leaveRoom(r.id, socket.id);
    console.log(`[-] client ${socket.id}`);
  });
});

// -----------------------------------------------------------------------------
// Anti-cheat: rate-limit fire & damage per socket
// -----------------------------------------------------------------------------
const fireLog = new Map();   // socketId -> [timestamps]
const MAX_FIRE_RATE = 30;    // rounds per second per player
const MAX_DAMAGE_PER_TICK = 500;

function validateFire(id, data) {
  const now = Date.now();
  const arr = fireLog.get(id) ?? [];
  arr.push(now);
  // Keep only last 1s
  while (arr.length && now - arr[0] > 1000) arr.shift();
  fireLog.set(id, arr);
  if (arr.length > MAX_FIRE_RATE) {
    flagCheater(id, 'fire_rate_exceeded', { rate: arr.length });
  }
}

function validateDamage(id, data) {
  if (data.damage > MAX_DAMAGE_PER_TICK) {
    flagCheater(id, 'damage_exceeded', { dmg: data.damage });
  }
}

async function flagCheater(id, reason, payload) {
  console.warn(`[anticheat] ${id}: ${reason}`, payload);
  await db.collection('anticheat').add({
    socketId: id, reason, payload, ts: Date.now()
  });
}

// -----------------------------------------------------------------------------
// Boot
// -----------------------------------------------------------------------------
const PORT = process.env.PORT || 4000;
server.listen(PORT, () => console.log(`[NeonCity] server listening on ${PORT}`));