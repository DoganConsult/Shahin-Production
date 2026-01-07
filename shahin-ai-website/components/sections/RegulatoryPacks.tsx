'use client';

import { motion } from 'framer-motion';
import { Shield, Building2, Lock, Cloud, Globe, CheckCircle2 } from 'lucide-react';

const packs = [
  {
    id: 'nca-ecc',
    name: 'NCA-ECC',
    nameAr: 'الضوابط الأساسية للأمن السيبراني',
    icon: Shield,
    controls: 114,
    description: 'Essential Cybersecurity Controls',
    regulator: 'National Cybersecurity Authority',
    color: 'from-emerald-500 to-emerald-600',
    featured: true,
  },
  {
    id: 'pdpl',
    name: 'PDPL',
    nameAr: 'نظام حماية البيانات الشخصية',
    icon: Lock,
    controls: 48,
    description: 'Personal Data Protection Law',
    regulator: 'SDAIA',
    color: 'from-blue-500 to-blue-600',
    featured: true,
  },
  {
    id: 'sama-csf',
    name: 'SAMA Cybersecurity',
    nameAr: 'إطار الأمن السيبراني لساما',
    icon: Building2,
    controls: 63,
    description: 'Banking Sector Requirements',
    regulator: 'Saudi Central Bank',
    color: 'from-purple-500 to-purple-600',
    featured: true,
  },
  {
    id: 'cloud',
    name: 'Cloud Controls',
    nameAr: 'ضوابط الحوسبة السحابية',
    icon: Cloud,
    controls: 38,
    description: 'Cloud Computing Guidelines',
    regulator: 'NCA / CITC',
    color: 'from-cyan-500 to-cyan-600',
    featured: false,
  },
  {
    id: 'iso27001',
    name: 'ISO 27001:2022',
    nameAr: 'آيزو 27001',
    icon: Globe,
    controls: 93,
    description: 'International Security Standard',
    regulator: 'ISO',
    color: 'from-teal-500 to-teal-600',
    featured: false,
  },
];

export default function RegulatoryPacks() {
  return (
    <section className="py-20 bg-slate-900">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-500/10 border border-emerald-500/30 rounded-full text-emerald-400 font-medium text-sm mb-6">
            🇸🇦 Saudi-First Compliance
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
            Pre-Built Regulatory Packs
          </h2>
          <p className="text-lg text-slate-300 max-w-2xl mx-auto mb-4">
            Start compliant from day one with ready-to-use control mappings for Saudi and international frameworks.
          </p>
          <p className="text-lg text-slate-400" dir="rtl">
            حزم تنظيمية جاهزة للتطبيق من اليوم الأول
          </p>
        </motion.div>

        {/* Featured Packs */}
        <div className="grid md:grid-cols-3 gap-6 mb-8">
          {packs.filter(p => p.featured).map((pack, index) => (
            <motion.div
              key={pack.id}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              className="group"
            >
              <div className="relative h-full p-6 bg-slate-800 rounded-2xl border border-slate-700 hover:border-emerald-500/50 transition-all overflow-hidden">
                {/* Gradient Background */}
                <div className={`absolute inset-0 bg-gradient-to-br ${pack.color} opacity-0 group-hover:opacity-10 transition-opacity`} />
                
                {/* Icon & Name */}
                <div className="flex items-start justify-between mb-4">
                  <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${pack.color} flex items-center justify-center shadow-lg`}>
                    <pack.icon className="w-7 h-7 text-white" />
                  </div>
                  <div className="text-right">
                    <div className="text-3xl font-bold text-white">{pack.controls}</div>
                    <div className="text-sm text-slate-400">Controls</div>
                  </div>
                </div>

                {/* Pack Name */}
                <h3 className="text-xl font-bold text-white mb-1">{pack.name}</h3>
                <p className="text-sm text-emerald-400 mb-2">{pack.description}</p>
                <p className="text-sm text-slate-500" dir="rtl">{pack.nameAr}</p>

                {/* Regulator */}
                <div className="mt-4 pt-4 border-t border-slate-700">
                  <div className="flex items-center justify-between">
                    <span className="text-xs text-slate-500">Regulator</span>
                    <span className="text-sm text-slate-300">{pack.regulator}</span>
                  </div>
                </div>

                {/* Status Badge */}
                <div className="mt-4 flex items-center gap-2 text-emerald-400">
                  <CheckCircle2 className="w-4 h-4" />
                  <span className="text-sm">Ready to Deploy</span>
                </div>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Other Packs */}
        <div className="grid md:grid-cols-2 gap-6">
          {packs.filter(p => !p.featured).map((pack, index) => (
            <motion.div
              key={pack.id}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
            >
              <div className="flex items-center gap-4 p-4 bg-slate-800/50 rounded-xl border border-slate-700 hover:border-slate-600 transition-all">
                <div className={`w-12 h-12 rounded-lg bg-gradient-to-br ${pack.color} flex items-center justify-center flex-shrink-0`}>
                  <pack.icon className="w-6 h-6 text-white" />
                </div>
                <div className="flex-1">
                  <h4 className="font-semibold text-white">{pack.name}</h4>
                  <p className="text-sm text-slate-400">{pack.description}</p>
                </div>
                <div className="text-right">
                  <div className="text-xl font-bold text-white">{pack.controls}</div>
                  <div className="text-xs text-slate-500">Controls</div>
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
          className="mt-12 text-center"
        >
          <p className="text-slate-400 mb-4">
            Need a custom framework pack? We can build it in weeks, not months.
          </p>
          <a
            href="#contact"
            className="inline-flex items-center gap-2 px-6 py-3 bg-emerald-500 hover:bg-emerald-600 text-white font-semibold rounded-xl transition-all"
          >
            Request Custom Pack
          </a>
        </motion.div>
      </div>
    </section>
  );
}
