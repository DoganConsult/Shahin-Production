'use client';

import { motion } from 'framer-motion';
import { AlertTriangle, Clock, FileX, Users, ArrowRight, CheckCircle2 } from 'lucide-react';

const problems = [
  {
    icon: FileX,
    pain: 'Audit Fatigue',
    painAr: 'إرهاق التدقيق',
    description: 'Endless requests, rework, inconsistent quality, business disruption.',
    solution: 'Automated evidence packs, 30-60% effort reduction',
    color: 'from-red-500 to-rose-500',
  },
  {
    icon: Clock,
    pain: 'Manual Evidence Chaos',
    painAr: 'فوضى جمع الأدلة يدويًا',
    description: 'Chasing people for documents, missing deadlines, version confusion.',
    solution: 'Auto-collect from ERP/IAM/SIEM with metadata tagging',
    color: 'from-orange-500 to-amber-500',
  },
  {
    icon: AlertTriangle,
    pain: 'Expansion Friction',
    painAr: 'صعوبة التوسع',
    description: 'Every new market/product triggers remapping and new evidence.',
    solution: 'Baseline + overlays, reuse across markets',
    color: 'from-yellow-500 to-orange-500',
  },
  {
    icon: Users,
    pain: 'Operational Fragility',
    painAr: 'هشاشة التشغيل',
    description: 'Controls exist on paper but failures detected late—often during audits.',
    solution: 'Continuous monitoring + early warning + workflow remediation',
    color: 'from-purple-500 to-pink-500',
  },
];

export default function ProblemCards() {
  return (
    <section className="py-20 bg-slate-50">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-red-100 rounded-full text-red-700 font-medium text-sm mb-6">
            Common Pain Points
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            We Know Your Challenges
          </h2>
          <p className="text-lg text-slate-600 max-w-2xl mx-auto mb-4">
            These problems drain resources, delay projects, and create risk—Shahin-AI solves them.
          </p>
          <p className="text-lg text-slate-500" dir="rtl">
            نحن نعرف تحدياتك ولدينا الحل
          </p>
        </motion.div>

        {/* Problem Cards */}
        <div className="grid md:grid-cols-2 gap-6">
          {problems.map((problem, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              className="group"
            >
              <div className="relative p-6 bg-white rounded-2xl border border-slate-200 hover:border-emerald-300 hover:shadow-lg transition-all h-full overflow-hidden">
                {/* Problem Side */}
                <div className="flex items-start gap-4 mb-6">
                  <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${problem.color} flex items-center justify-center flex-shrink-0`}>
                    <problem.icon className="w-7 h-7 text-white" />
                  </div>
                  <div>
                    <h3 className="text-xl font-bold text-slate-900 mb-1">{problem.pain}</h3>
                    <p className="text-sm text-slate-500" dir="rtl">{problem.painAr}</p>
                  </div>
                </div>

                <p className="text-slate-600 mb-6">{problem.description}</p>

                {/* Arrow Divider */}
                <div className="flex items-center gap-3 mb-4">
                  <div className="flex-1 h-px bg-slate-200" />
                  <ArrowRight className="w-5 h-5 text-emerald-500" />
                  <div className="flex-1 h-px bg-slate-200" />
                </div>

                {/* Solution Side */}
                <div className="flex items-start gap-3 p-4 bg-emerald-50 rounded-xl border border-emerald-200">
                  <CheckCircle2 className="w-5 h-5 text-emerald-600 flex-shrink-0 mt-0.5" />
                  <p className="text-emerald-800 font-medium">{problem.solution}</p>
                </div>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Bottom Stats */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="mt-16 grid grid-cols-2 md:grid-cols-4 gap-6"
        >
          {[
            { value: '30-60%', label: 'Audit Effort Reduction' },
            { value: '80%', label: 'Evidence Time Saved' },
            { value: '50%', label: 'Faster Market Entry' },
            { value: '90%', label: 'Repeat Finding Reduction' },
          ].map((stat, index) => (
            <div key={index} className="text-center p-6 bg-white rounded-xl border border-slate-200">
              <div className="text-3xl font-bold text-emerald-600 mb-2">{stat.value}</div>
              <div className="text-sm text-slate-600">{stat.label}</div>
            </div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
