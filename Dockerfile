# Multi-stage Dockerfile for NeonCity Unity Linux server build
# Build context = repo root
#   docker build -t neoncity-game -f Dockerfile .

# ---- 1) Build Unity Linux64 server ----
FROM unityci/editor:ubuntu-6000.0.23f1-base-3.1.0 AS builder
WORKDIR /project
COPY . .

# Authorize Unity (free personal license)
RUN /opt/unity/Editor/Unity \
      -batchmode -quit -nographics -accept-apiupdate \
      -projectPath /project \
      -buildTarget Linux64 \
      -executeMethod BuildScript.Linux64 \
      -logFile -

# ---- 2) Runtime image (Ubuntu) ----
FROM ubuntu:22.04
RUN apt-get update && apt-get install -y --no-install-recommends \
        libglu1-mesa libxi6 libxrandr2 libxcursor1 libxinerama1 \
        ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /game
COPY --from=builder /project/build/Linux /game

EXPOSE 7777/udp
EXPOSE 7777/tcp

# Default args: headless dedicated server
ENV UNITY_SERVER_MODE=1

ENTRYPOINT ["/game/NeonCity.x86_64", "-batchmode", "-nographics", "-server"]