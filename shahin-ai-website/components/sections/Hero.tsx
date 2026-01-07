'use client';

import { motion } from 'framer-motion';
import { ArrowRight, Play, Shield, Zap, Globe } from 'lucide-react';

export default function Hero() {
  return (
    <section className="relative min-h-screen bg-gradient-to-br from-slate-900 via-emerald-900 to-slate-900 overflow-hidden">
      {/* Background Pattern */}
      <div className="absolute inset-0 bg-[url('/grid.svg')] opacity-10" />
      <div className="absolute inset-0 bg-gradient-to-t from-slate-900/80 to-transparent" />
      
      {/* Floating Elements */}
      <motion.div 
        className="absolute top-20 left-10 w-72 h-72 bg-emerald-500/20 rounded-full blur-3xl"
        animate={{ scale: [1, 1.2, 1], opacity: [0.3, 0.5, 0.3] }}
        transition={{ duration: 8, repeat: Infinity }}
      />
      <motion.div 
        className="absolute bottom-20 right-10 w-96 h-96 bg-teal-500/20 rounded-full blur-3xl"
        animate={{ scale: [1.2, 1, 1.2], opacity: [0.3, 0.5, 0.3] }}
        transition={{ duration: 10, repeat: Infinity }}
      />

      <div className="relative container mx-auto px-6 pt-32 pb-20">
        <div className="max-w-5xl mx-auto text-center">
          {/* Badge */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-500/10 border border-emerald-500/30 rounded-full mb-8"
          >
            <Shield className="w-4 h-4 text-emerald-400" />
            <span className="text-emerald-300 text-sm font-medium">
              Built for Saudi Arabia & MENA Region
            </span>
          </motion.div>

          {/* Main Headline - Arabic */}
          <motion.h1
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.1 }}
            className="text-4xl md:text-5xl font-bold text-white mb-4 font-arabic"
            dir="rtl"
          >
            خريطة تحكم واحدة لكل الأسواق
          </motion.h1>

          {/* Subheadline - English */}
          <motion.h2
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.2 }}
            className="text-3xl md:text-4xl font-bold bg-gradient-to-r from-emerald-400 via-teal-400 to-cyan-400 bg-clip-text text-transparent mb-6"
          >
            One Control Map for All Markets
          </motion.h2>

          {/* Description */}
          <motion.p
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.3 }}
            className="text-xl text-slate-300 mb-8 max-w-3xl mx-auto leading-relaxed"
          >
            Automate evidence collection, continuously monitor controls, and drive remediation 
            through workflows—so audits become faster and compliance stays predictable across 
            <span className="text-emerald-400 font-semibold"> 120+ regulators</span> and 
            <span className="text-emerald-400 font-semibold"> 240+ frameworks</span>.
          </motion.p>

          {/* Operating Sentence */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.4 }}
            className="bg-slate-800/50 backdrop-blur border border-slate-700 rounded-2xl p-6 mb-10 max-w-4xl mx-auto"
          >
            <p className="text-lg text-slate-200 font-mono">
              <span className="text-emerald-400">MAP</span> it, 
              <span className="text-teal-400"> APPLY</span> it, 
              <span className="text-cyan-400"> PROVE</span> it — 
              <span className="text-blue-400">WATCH</span> exceptions and 
              <span className="text-purple-400"> FIX</span> gaps; store in 
              <span className="text-pink-400"> VAULT</span>.
            </p>
          </motion.div>

          {/* CTA Buttons */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.5 }}
            className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-16"
          >
            <a
              href="#demo"
              className="group flex items-center gap-2 px-8 py-4 bg-gradient-to-r from-emerald-500 to-teal-500 text-white font-semibold rounded-xl hover:from-emerald-600 hover:to-teal-600 transition-all shadow-lg shadow-emerald-500/25"
            >
              Request Demo
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </a>
            <a
              href="#video"
              className="group flex items-center gap-2 px-8 py-4 bg-slate-800 border border-slate-700 text-white font-semibold rounded-xl hover:bg-slate-700 transition-all"
            >
              <Play className="w-5 h-5" />
              Watch Video
            </a>
          </motion.div>

          {/* Feature Pills */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.6 }}
            className="flex flex-wrap items-center justify-center gap-3"
          >
            {[
              { icon: Shield, text: 'NCA-ECC Ready' },
              { icon: Globe, text: 'PDPL Compliant' },
              { icon: Zap, text: 'SAMA Cybersecurity' },
            ].map((item, index) => (
              <div
                key={index}
                className="flex items-center gap-2 px-4 py-2 bg-slate-800/50 border border-slate-700 rounded-full"
              >
                <item.icon className="w-4 h-4 text-emerald-400" />
                <span className="text-slate-300 text-sm">{item.text}</span>
              </div>
            ))}
          </motion.div>
        </div>

        {/* Dashboard Preview Placeholder */}
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 0.7 }}
          className="mt-16 max-w-5xl mx-auto"
        >
          <div className="relative rounded-2xl overflow-hidden border border-slate-700 bg-slate-800/50 backdrop-blur shadow-2xl">
            <div className="absolute inset-0 bg-gradient-to-t from-slate-900 via-transparent to-transparent z-10" />
            <div className="aspect-video bg-gradient-to-br from-slate-800 to-slate-900 flex items-center justify-center">
              <div className="text-center z-20">
                <div className="w-20 h-20 mx-auto mb-4 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-500 flex items-center justify-center">
                  <Play className="w-8 h-8 text-white" />
                </div>
                <p className="text-slate-400">Platform Preview Coming Soon</p>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
