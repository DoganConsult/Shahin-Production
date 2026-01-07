'use client';

import { motion } from 'framer-motion';
import { Map, Settings, FileCheck2, Eye, Wrench, Lock } from 'lucide-react';

const modules = [
  {
    id: 'map',
    name: 'MAP',
    nameAr: 'خريطة',
    icon: Map,
    description: 'Requirement → Objective → Control mapping',
    descriptionAr: 'ربط المتطلبات → الأهداف → الضوابط',
    color: 'from-emerald-500 to-emerald-600',
    features: ['Canonical control library', 'Objective taxonomy', 'Framework crosswalks'],
  },
  {
    id: 'apply',
    name: 'APPLY',
    nameAr: 'تطبيق',
    icon: Settings,
    description: 'Scope and applicability rules',
    descriptionAr: 'قواعد النطاق والانطباق حسب السوق والقطاع',
    color: 'from-teal-500 to-teal-600',
    features: ['Sector overlays', 'Jurisdiction rules', 'Data classification'],
  },
  {
    id: 'prove',
    name: 'PROVE',
    nameAr: 'إثبات',
    icon: FileCheck2,
    description: 'Evidence packs, testing, audit-ready',
    descriptionAr: 'حزم الأدلة والاختبارات وتجهيز ملفات التدقيق',
    color: 'from-cyan-500 to-cyan-600',
    features: ['Auto-collect evidence', 'Standard test scripts', 'Audit packages'],
  },
  {
    id: 'watch',
    name: 'WATCH',
    nameAr: 'مراقبة',
    icon: Eye,
    description: 'Continuous monitoring (KRIs/KPIs), alerts',
    descriptionAr: 'مراقبة مستمرة للمؤشرات والتنبيهات',
    color: 'from-blue-500 to-blue-600',
    features: ['Real-time dashboards', 'Threshold alerts', 'Trend analysis'],
  },
  {
    id: 'fix',
    name: 'FIX',
    nameAr: 'معالجة',
    icon: Wrench,
    description: 'Remediation workflow (tickets, SLAs, exceptions)',
    descriptionAr: 'سير عمل المعالجات والاستثناءات',
    color: 'from-purple-500 to-purple-600',
    features: ['ITSM integration', 'SLA tracking', 'Escalation rules'],
  },
  {
    id: 'vault',
    name: 'VAULT',
    nameAr: 'خزنة',
    icon: Lock,
    description: 'Evidence repository with versioning and audit trail',
    descriptionAr: 'مستودع أدلة مع إصدار وتدقيق تغييرات',
    color: 'from-pink-500 to-pink-600',
    features: ['Immutable storage', 'Version control', 'Access controls'],
  },
];

export default function DifferentiatorGrid() {
  return (
    <section className="py-20 bg-gradient-to-b from-slate-50 to-white">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-100 rounded-full text-emerald-700 font-medium text-sm mb-6">
            6 Integrated Modules
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            The Shahin-AI Operating System
          </h2>
          <p className="text-lg text-slate-600 max-w-3xl mx-auto mb-4">
            A modular framework that makes compliance repeatable, measurable, and audit-ready by design.
          </p>
          <p className="text-lg text-slate-500" dir="rtl">
            نظام تشغيل شاهين للحوكمة والامتثال
          </p>
        </motion.div>

        {/* Modules Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {modules.map((module, index) => (
            <motion.div
              key={module.id}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              className="group"
            >
              <div className="relative h-full p-6 bg-white rounded-2xl border border-slate-200 hover:border-emerald-300 hover:shadow-xl transition-all overflow-hidden">
                {/* Background Gradient on Hover */}
                <div className={`absolute inset-0 bg-gradient-to-br ${module.color} opacity-0 group-hover:opacity-5 transition-opacity`} />
                
                {/* Module Header */}
                <div className="flex items-start gap-4 mb-4">
                  <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${module.color} flex items-center justify-center group-hover:scale-110 transition-transform shadow-lg`}>
                    <module.icon className="w-7 h-7 text-white" />
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h3 className="text-2xl font-bold text-slate-900">{module.name}</h3>
                      <span className="text-sm text-slate-400" dir="rtl">{module.nameAr}</span>
                    </div>
                    <p className="text-slate-600 text-sm">{module.description}</p>
                  </div>
                </div>

                {/* Arabic Description */}
                <p className="text-sm text-slate-500 mb-4" dir="rtl">
                  {module.descriptionAr}
                </p>

                {/* Features */}
                <ul className="space-y-2">
                  {module.features.map((feature, fIndex) => (
                    <li key={fIndex} className="flex items-center gap-2 text-sm text-slate-600">
                      <div className={`w-1.5 h-1.5 rounded-full bg-gradient-to-br ${module.color}`} />
                      {feature}
                    </li>
                  ))}
                </ul>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Operating Sentence */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="mt-16"
        >
          <div className="bg-slate-900 rounded-2xl p-8 text-center">
            <p className="text-lg md:text-xl text-white font-mono">
              "<span className="text-emerald-400">MAP</span> it, 
              <span className="text-teal-400"> APPLY</span> it, 
              <span className="text-cyan-400"> PROVE</span> it — 
              <span className="text-blue-400">WATCH</span> exceptions and 
              <span className="text-purple-400"> FIX</span> gaps; store in 
              <span className="text-pink-400"> VAULT</span>."
            </p>
            <p className="text-slate-400 mt-4 text-sm" dir="rtl">
              «اربطها، طبّقها، أثبتها—راقب الاستثناءات وعالج الفجوات؛ واحفظ كل شيء في الخزنة.»
            </p>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
