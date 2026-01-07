import Header from '@/components/layout/Header';
import Footer from '@/components/layout/Footer';
import Hero from '@/components/sections/Hero';
import TrustStrip from '@/components/sections/TrustStrip';
import StatsSection from '@/components/sections/StatsSection';
import ProblemCards from '@/components/sections/ProblemCards';
import DifferentiatorGrid from '@/components/sections/DifferentiatorGrid';
import HowItWorks from '@/components/sections/HowItWorks';
import RegulatoryPacks from '@/components/sections/RegulatoryPacks';
import Industries from '@/components/sections/Industries';
import PlatformPreview from '@/components/sections/PlatformPreview';
import Testimonials from '@/components/sections/Testimonials';
import PricingPreview from '@/components/sections/PricingPreview';
import Resources from '@/components/sections/Resources';
import About from '@/components/sections/About';
import CtaBanner from '@/components/sections/CtaBanner';
import OnboardingQuestionnaire from '@/components/sections/OnboardingQuestionnaire';

export default function HomePage() {
  return (
    <main className="min-h-screen bg-white">
      <Header />
      <Hero />
      <TrustStrip />
      <StatsSection />
      <ProblemCards />
      <DifferentiatorGrid />
      <HowItWorks />
      <RegulatoryPacks />
      <Industries />
      <PlatformPreview />
      <Testimonials />
      <PricingPreview />
      <Resources />
      <About />
      <CtaBanner />
      <OnboardingQuestionnaire />
      <Footer />
    </main>
  );
}
