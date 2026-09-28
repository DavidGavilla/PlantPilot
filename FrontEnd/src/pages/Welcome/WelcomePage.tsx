import { HeroSection } from './sections/HeroSection'
import { AudienceSplitSection } from './sections/AudienceSplitSection'
import { CapabilitiesGrid } from './sections/CapabilitiesGrid'
import { FutureReadyBand } from './sections/FutureReadyBand'
import { ClosingCtaSection } from './sections/ClosingCtaSection'

export function WelcomePage() {
  return (
    <main id="main-content">
      <HeroSection />
      <AudienceSplitSection />
      <CapabilitiesGrid />
      <FutureReadyBand />
      <ClosingCtaSection />
    </main>
  )
}
