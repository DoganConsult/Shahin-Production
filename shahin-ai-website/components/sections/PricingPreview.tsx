'use client';

import { motion } from 'framer-motion';
import { Check, ArrowRight, Zap, Shield, Rocket } from 'lucide-react';

const tiers = [
  {
    id: 'foundation',
    name: 'Foundation',
    nameAr: 'التأسيس',
    icon: Shield,
    duration: '60-90 days',
    description: 'Audit-ready baseline and scope logic',
    outcome: 'Get audit-ready with a reusable control map',
    color: 'from-emerald-500 to-teal-500',
    features: [
      'Canonical control library',
      'Baseline + overlays setup',
      'Evidence pack standards',
      'Controlled repository structure',
      'Initial audit response workflow',
    ],
    popular: false,
  },
  {
    id: 'assurance-ops',
    name: 'Assurance Ops',
    nameAr: 'عمليات الضمان',
    icon: Zap,
    duration: '90-180 days',
    description: 'Predictable audit operations with KPIs',
    outcome: 'Workflow-driven remediation with measurable SLAs',
    color: 'from-teal-500 to-cyan-500',
    features: [
      'Everything in Foundation',
      'Automated evidence collection',
      'Standard test scripts + workpapers',
      'ITSM integration (tickets, SLAs)',
      'KPI reporting & dashboards',
    ],
    popular: true,
  },
  {
    id: 'continuous',
    name: 'Continuous Assurance',
    nameAr: 'الضمان المستمر',
    icon: Rocket,
    duration: '6-12 months',
    description: 'Always-on control health + early warning',
    outcome: 'Real-time compliance with proactive remediation',
    color: 'from-cyan-500 to-blue-500',
    features: [
      'Everything in Assurance Ops',
      'Controls-as-code (20-50 controls)',
      'KRIs/KPIs + alerting',
      'Exception automation',
      'Resilience operating model',
    ],
    popular: false,
  },
];

export default function PricingPreview() {
  return (
    <section className="py-20 bg-white">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-100 rounded-full text-emerald-700 font-medium text-sm mb-6">
            Outcome-Based Pricing
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            Choose Your Maturity Level
          </h2>
          <p className="text-lg text-slate-600 max-w-2xl mx-auto mb-4">
            We price based on outcomes, not licenses. Pick the tier that matches your goals.
          </p>
          <p className="text-lg text-slate-500" dir="rtl">
            نسعّر بناءً على النتائج، اختر المستوى الذي يناسب أهدافك
          </p>
        </motion.div>

        {/* Pricing Cards */}
        <div className="grid md:grid-cols-3 gap-8">
          {tiers.map((tier, index) => (
            <motion.div
              key={tier.id}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              className={`relative ${tier.popular ? 'md:-mt-4 md:mb-4' : ''}`}
            >
              {/* Popular Badge */}
              {tier.popular && (
                <div className="absolute -top-4 left-1/2 -translate-x-1/2 z-10">
                  <div className="px-4 py-1 bg-gradient-to-r from-emerald-500 to-teal-500 text-white text-sm font-semibold rounded-full shadow-lg">
                    Most Popular
                  </div>
                </div>
              )}

              <div className={`relative h-full p-8 rounded-2xl border-2 transition-all ${
                tier.popular 
                  ? 'bg-gradient-to-b from-slate-50 to-white border-emerald-300 shadow-xl' 
                  : 'bg-white border-slate-200 hover:border-slate-300'
              }`}>
                {/* Icon */}
                <div className={`w-16 h-16 mb-6 rounded-2xl bg-gradient-to-br ${tier.color} flex items-center justify-center`}>
                  <tier.icon className="w-8 h-8 text-white" />
                </div>

                {/* Tier Name */}
                <h3 className="text-2xl font-bold text-slate-900 mb-1">{tier.name}</h3>
                <p className="text-emerald-600 font-medium mb-4" dir="rtl">{tier.nameAr}</p>

                {/* Duration */}
                <div className="text-3xl font-bold text-slate-900 mb-2">{tier.duration}</div>
                <p className="text-slate-600 mb-6">{tier.description}</p>

                {/* Outcome */}
                <div className="p-4 bg-slate-50 rounded-xl mb-6 border border-slate-200">
                  <p className="text-sm text-slate-700 font-medium">{tier.outcome}</p>
                </div>

                {/* Features */}
                <ul className="space-y-3 mb-8">
                  {tier.features.map((feature, fIndex) => (
                    <li key={fIndex} className="flex items-start gap-3">
                      <Check className="w-5 h-5 text-emerald-500 flex-shrink-0 mt-0.5" />
                      <span className="text-slate-600">{feature}</span>
                    </li>
                  ))}
                </ul>

                {/* CTA */}
                <a
                  href="#contact"
                  className={`flex items-center justify-center gap-2 w-full py-4 rounded-xl font-semibold transition-all ${
                    tier.popular
                      ? 'bg-gradient-to-r from-emerald-500 to-teal-500 text-white hover:from-emerald-600 hover:to-teal-600'
                      : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
                  }`}
                >
                  Get Started
                  <ArrowRight className="w-5 h-5" />
                </a>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Enterprise Note */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="mt-16 text-center"
        >
          <div className="inline-flex flex-col items-center gap-3 p-6 bg-slate-50 rounded-2xl border border-slate-200">
            <p className="text-slate-600">
              Need enterprise-scale deployment across multiple entities and markets?
            </p>
            <a
              href="#contact"
              className="inline-flex items-center gap-2 text-emerald-600 font-semibold hover:text-emerald-700"
            >
              Talk to our Enterprise Team
              <ArrowRight className="w-4 h-4" />
            </a>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
