// =============================================================================
// NeonCity — Core / IService.cs
// -----------------------------------------------------------------------------
// Minimal contract every long-lived service implements. The GameManager holds
// the registry and calls Initialize/Shutdown in lifecycle order.
//
// Why? Decouples systems so we can swap a service (e.g. mock AudioService for
// tests) without touching call-sites.
// =============================================================================

namespace NeonCity.Core
{
    public interface IService
    {
        void Initialize();
        void Shutdown();
    }
}