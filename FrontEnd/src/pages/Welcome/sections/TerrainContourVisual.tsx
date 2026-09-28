// Intentional abstract visual (layered SVG contour lines, no photo) evoking satellite
// terrain mapping - not a fake product screenshot, no fabricated data or UI chrome.
export function TerrainContourVisual() {
  const rings = [92, 78, 64, 50, 36, 22]

  return (
    <svg
      viewBox="0 0 400 400"
      preserveAspectRatio="xMidYMid slice"
      className="h-full w-full"
      role="presentation"
      aria-hidden="true"
    >
      <defs>
        <radialGradient id="terrainBase" cx="35%" cy="35%" r="75%">
          <stop offset="0%" stopColor="#1f764e" />
          <stop offset="100%" stopColor="#0a2419" />
        </radialGradient>
      </defs>
      <rect width="400" height="400" fill="url(#terrainBase)" />
      {rings.map((r, index) => (
        <circle
          key={r}
          cx="140"
          cy="150"
          r={r * 2}
          fill="none"
          stroke="rgba(219, 242, 226, 0.18)"
          strokeWidth={index === rings.length - 1 ? 2 : 1}
        />
      ))}
      <circle cx="290" cy="270" r="10" fill="#86cea6" opacity="0.9" />
      <circle cx="290" cy="270" r="26" fill="none" stroke="#86cea6" strokeOpacity="0.35" strokeWidth="1.5" />
    </svg>
  )
}
