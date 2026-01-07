'use client';

import { motion } from 'framer-motion';
import { UserPlus, Search, Rocket, ArrowRight } from 'lucide-react';

const steps = [
  {
    number: '01',
    icon: UserPlus,
    title: 'ONBOARD',
    titleAr: 'التأهيل',
    description: '12-step wizard profiles your organization, assets, and regulatory scope.',
    descriptionAr: 'معالج من 12 خطوة يرصد ملامح مؤسستك والأصول والنطاق التنظيمي',
    details: [
      'Organization identity & tenancy',
      'Regulatory & framework applicability',
      'Scope definition (entities, systems, data)',
      'Teams, roles, and access setup',
    ],
    color: 'from-emerald-500 to-teal-500',
  },
  {
    number: '02',
    icon: Search,
    title: 'ASSESS',
    titleAr: 'التقييم',
    description: 'AI-driven gap analysis identifies what applies and what is missing.',
    descriptionAr: 'تحليل ذكي للفجوات يحدد ما ينطبق وما ينقص',
    details: [
      'Baseline + overlay control generation',
      'Automatic applicability rules',
      'Evidence pack assignments',
      'Gap identification & remediation plan',
    ],
    color: 'from-teal-500 to-cyan-500',
  },
  {
    number: '03',
    icon: Rocket,
    title: 'COMPLY',
    titleAr: 'الامتثال',
    description: 'Continuous monitoring & evidence automation keeps you audit-ready.',
    descriptionAr: 'مراقبة مستمرة وأتمتة الأدلة تبقيك جاهزًا للتدقيق',
    details: [
      'Auto-collect evidence from ERP/IAM/SIEM',
      'Continuous control monitoring (KRIs)',
      'Workflow-driven remediation',
      'Real-time dashboards & reporting',
    ],
    color: 'from-cyan-500 to-blue-500',
  },
];

export default function HowItWorks() {
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
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-slate-100 rounded-full text-slate-700 font-medium text-sm mb-6">
            Simple 3-Step Process
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            How Shahin-AI Works
          </h2>
          <p className="text-lg text-slate-600 max-w-2xl mx-auto mb-4">
            From onboarding to continuous compliance in weeks, not months.
          </p>
          <p className="text-lg text-slate-500" dir="rtl">
            من التأهيل إلى الامتثال المستمر في أسابيع وليس أشهر
          </p>
        </motion.div>

        {/* Steps */}
        <div className="grid md:grid-cols-3 gap-8">
          {steps.map((step, index) => (
            <motion.div
              key={step.number}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.2 }}
              className="relative"
            >
              {/* Connector Arrow (not on last item) */}
              {index < steps.length - 1 && (
                <div className="hidden md:block absolute top-24 -right-4 z-10">
                  <ArrowRight className="w-8 h-8 text-slate-300" />
                </div>
              )}

              <div className="relative p-8 bg-slate-50 rounded-2xl border border-slate-200 hover:border-emerald-300 hover:shadow-lg transition-all h-full">
                {/* Step Number */}
                <div className={`absolute -top-4 -left-2 w-12 h-12 rounded-xl bg-gradient-to-br ${step.color} flex items-center justify-center text-white font-bold text-lg shadow-lg`}>
                  {step.number}
                </div>

                {/* Icon */}
                <div className={`w-16 h-16 mb-6 rounded-2xl bg-gradient-to-br ${step.color} flex items-center justify-center mx-auto`}>
                  <step.icon className="w-8 h-8 text-white" />
                </div>

                {/* Title */}
                <div className="text-center mb-4">
                  <h3 className="text-2xl font-bold text-slate-900 mb-1">{step.title}</h3>
                  <p className="text-emerald-600 font-medium" dir="rtl">{step.titleAr}</p>
                </div>

                {/* Description */}
                <p className="text-slate-600 text-center mb-4">{step.description}</p>
                <p className="text-sm text-slate-500 text-center mb-6" dir="rtl">{step.descriptionAr}</p>

                {/* Details */}
                <ul className="space-y-2">
                  {step.details.map((detail, dIndex) => (
                    <li key={dIndex} className="flex items-start gap-2 text-sm text-slate-600">
                      <div className={`w-1.5 h-1.5 mt-2 rounded-full bg-gradient-to-br ${step.color} flex-shrink-0`} />
                      {detail}
                    </li>
                  ))}
                </ul>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Timeline Indicator */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="mt-16"
        >
          <div className="bg-gradient-to-r from-emerald-50 via-teal-50 to-cyan-50 rounded-2xl p-8 border border-emerald-200">
            <div className="grid md:grid-cols-3 gap-8 text-center">
              <div>
                <div className="text-3xl font-bold text-emerald-600 mb-2">60-90 days</div>
                <div className="text-slate-600">Foundation Tier</div>
                <div className="text-sm text-slate-500">Audit-ready baseline</div>
              </div>
              <div>
                <div className="text-3xl font-bold text-teal-600 mb-2">90-180 days</div>
                <div className="text-slate-600">Assurance Ops Tier</div>
                <div className="text-sm text-slate-500">Predictable operations</div>
              </div>
              <div>
                <div className="text-3xl font-bold text-cyan-600 mb-2">6-12 months</div>
                <div className="text-slate-600">Continuous Assurance</div>
                <div className="text-sm text-slate-500">Always-on control health</div>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
