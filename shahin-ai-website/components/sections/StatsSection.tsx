'use client';

import { motion } from 'framer-motion';
import { Building2, Globe2, FileCheck, ShieldCheck, Clock, TrendingDown } from 'lucide-react';

const stats = [
  {
    icon: Building2,
    value: '120+',
    label: 'Local Regulators',
    labelAr: 'جهة تنظيمية محلية',
    color: 'from-emerald-500 to-teal-500',
  },
  {
    icon: Globe2,
    value: '30+',
    label: 'International Regulators',
    labelAr: 'جهة تنظيمية دولية',
    color: 'from-teal-500 to-cyan-500',
  },
  {
    icon: FileCheck,
    value: '240+',
    label: 'Frameworks Supported',
    labelAr: 'إطار تنظيمي مدعوم',
    color: 'from-cyan-500 to-blue-500',
  },
  {
    icon: ShieldCheck,
    value: '4,000+',
    label: 'Controls Mapped',
    labelAr: 'ضابط رقابي مربوط',
    color: 'from-blue-500 to-purple-500',
  },
  {
    icon: Clock,
    value: '80%',
    label: 'Evidence Time Saved',
    labelAr: 'توفير في وقت جمع الأدلة',
    color: 'from-purple-500 to-pink-500',
  },
  {
    icon: TrendingDown,
    value: '60%',
    label: 'Audit Effort Reduction',
    labelAr: 'تخفيض في جهد التدقيق',
    color: 'from-pink-500 to-rose-500',
  },
];

export default function StatsSection() {
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
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            Enterprise-Scale Compliance Coverage
          </h2>
          <p className="text-lg text-slate-600 max-w-2xl mx-auto" dir="rtl">
            تغطية امتثال على مستوى المؤسسات
          </p>
        </motion.div>

        {/* Stats Grid */}
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-6">
          {stats.map((stat, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              className="group"
            >
              <div className="relative p-6 bg-slate-50 rounded-2xl border border-slate-200 hover:border-emerald-300 hover:shadow-lg transition-all h-full">
                {/* Icon */}
                <div className={`w-12 h-12 mb-4 rounded-xl bg-gradient-to-br ${stat.color} flex items-center justify-center group-hover:scale-110 transition-transform`}>
                  <stat.icon className="w-6 h-6 text-white" />
                </div>
                
                {/* Value */}
                <div className="text-3xl font-bold text-slate-900 mb-2">
                  {stat.value}
                </div>
                
                {/* Labels */}
                <div className="text-sm text-slate-600 font-medium">
                  {stat.label}
                </div>
                <div className="text-xs text-slate-500 mt-1" dir="rtl">
                  {stat.labelAr}
                </div>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Bottom CTA */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="mt-16 text-center"
        >
          <p className="text-slate-600 mb-6">
            One canonical control library. Multiple precise compliance views.
          </p>
          <div className="inline-flex items-center gap-2 px-6 py-3 bg-emerald-50 border border-emerald-200 rounded-full text-emerald-700 font-medium">
            <ShieldCheck className="w-5 h-5" />
            Map once, reuse everywhere
          </div>
        </motion.div>
      </div>
    </section>
  );
}
