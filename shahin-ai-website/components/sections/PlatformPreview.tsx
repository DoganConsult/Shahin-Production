'use client';

import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { LayoutDashboard, ShieldAlert, FileCheck, GitBranch, Play } from 'lucide-react';

const tabs = [
  {
    id: 'dashboard',
    name: 'Dashboard',
    nameAr: 'لوحة التحكم',
    icon: LayoutDashboard,
    description: 'Real-time compliance score, risk heat map, and KRI tracking',
    features: ['Compliance Score', 'Risk Heat Map', 'Control Health', 'Pending Tasks'],
  },
  {
    id: 'risk',
    name: 'Risk Management',
    nameAr: 'إدارة المخاطر',
    icon: ShieldAlert,
    description: 'Risk register, probability/impact matrix, and mitigation tracking',
    features: ['Risk Matrix', 'Treatment Plans', 'Owner Assignment', 'Trend Analysis'],
  },
  {
    id: 'evidence',
    name: 'Evidence Library',
    nameAr: 'مكتبة الأدلة',
    icon: FileCheck,
    description: 'Automated collection, version control, and audit packaging',
    features: ['Auto-Collection', 'Expiration Alerts', 'Audit Packages', 'Access Control'],
  },
  {
    id: 'workflow',
    name: 'Workflow Engine',
    nameAr: 'محرك سير العمل',
    icon: GitBranch,
    description: 'Approval chains, task inbox, and escalation management',
    features: ['Task Inbox', 'Approval Chains', 'SLA Tracking', 'Escalations'],
  },
];

export default function PlatformPreview() {
  const [activeTab, setActiveTab] = useState('dashboard');
  const activeTabData = tabs.find(t => t.id === activeTab);

  return (
    <section className="py-20 bg-gradient-to-b from-slate-900 to-slate-800">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-center mb-12"
        >
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-500/10 border border-emerald-500/30 rounded-full text-emerald-400 font-medium text-sm mb-6">
            Platform Preview
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
            Experience the Platform
          </h2>
          <p className="text-lg text-slate-300 max-w-2xl mx-auto">
            See how Shahin-AI makes compliance measurable, repeatable, and audit-ready.
          </p>
        </motion.div>

        {/* Tab Navigation */}
        <div className="flex flex-wrap justify-center gap-3 mb-8">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-2 px-5 py-3 rounded-xl font-medium transition-all ${
                activeTab === tab.id
                  ? 'bg-gradient-to-r from-emerald-500 to-teal-500 text-white shadow-lg'
                  : 'bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 border border-slate-700'
              }`}
            >
              <tab.icon className="w-5 h-5" />
              <span className="hidden sm:inline">{tab.name}</span>
            </button>
          ))}
        </div>

        {/* Preview Area */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="relative"
        >
          <div className="relative rounded-2xl overflow-hidden border border-slate-700 bg-slate-800 shadow-2xl">
            {/* Browser Chrome */}
            <div className="flex items-center gap-2 px-4 py-3 bg-slate-900 border-b border-slate-700">
              <div className="flex gap-1.5">
                <div className="w-3 h-3 rounded-full bg-red-500" />
                <div className="w-3 h-3 rounded-full bg-yellow-500" />
                <div className="w-3 h-3 rounded-full bg-green-500" />
              </div>
              <div className="flex-1 mx-4">
                <div className="flex items-center gap-2 px-4 py-1.5 bg-slate-800 rounded-lg text-sm text-slate-400">
                  <span>app.shahin-ai.com/{activeTab}</span>
                </div>
              </div>
            </div>

            {/* Content Area */}
            <AnimatePresence mode="wait">
              <motion.div
                key={activeTab}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                transition={{ duration: 0.3 }}
                className="aspect-video bg-gradient-to-br from-slate-800 to-slate-900 flex items-center justify-center p-8"
              >
                <div className="text-center max-w-lg">
                  {/* Icon */}
                  <div className="w-20 h-20 mx-auto mb-6 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-500 flex items-center justify-center">
                    {activeTabData && <activeTabData.icon className="w-10 h-10 text-white" />}
                  </div>

                  {/* Title */}
                  <h3 className="text-2xl font-bold text-white mb-2">{activeTabData?.name}</h3>
                  <p className="text-emerald-400 mb-4" dir="rtl">{activeTabData?.nameAr}</p>
                  <p className="text-slate-400 mb-6">{activeTabData?.description}</p>

                  {/* Features */}
                  <div className="flex flex-wrap justify-center gap-2 mb-8">
                    {activeTabData?.features.map((feature, index) => (
                      <span
                        key={index}
                        className="px-3 py-1 bg-slate-700 text-slate-300 rounded-full text-sm"
                      >
                        {feature}
                      </span>
                    ))}
                  </div>

                  {/* Play Button */}
                  <button className="inline-flex items-center gap-2 px-6 py-3 bg-emerald-500 hover:bg-emerald-600 text-white font-semibold rounded-xl transition-all">
                    <Play className="w-5 h-5" />
                    Watch Demo Video
                  </button>
                </div>
              </motion.div>
            </AnimatePresence>
          </div>

          {/* Decorative Glow */}
          <div className="absolute -inset-4 bg-gradient-to-r from-emerald-500/20 via-teal-500/20 to-cyan-500/20 rounded-3xl blur-2xl -z-10" />
        </motion.div>

        {/* Screenshot Coming Soon Note */}
        <motion.div
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          className="mt-8 text-center"
        >
          <p className="text-slate-500 text-sm">
            Live platform screenshots coming soon • Request a personalized demo
          </p>
        </motion.div>
      </div>
    </section>
  );
}
