'use client';

import { motion } from 'framer-motion';
import { ArrowRight, Calendar, Play, Shield } from 'lucide-react';

export default function CtaBanner() {
  return (
    <section className="py-20 bg-gradient-to-br from-emerald-600 via-teal-600 to-cyan-600 relative overflow-hidden">
      {/* Background Pattern */}
      <div className="absolute inset-0 bg-[url('/grid.svg')] opacity-10" />
      
      {/* Floating Shapes */}
      <motion.div
        className="absolute top-10 left-10 w-64 h-64 bg-white/10 rounded-full blur-3xl"
        animate={{ scale: [1, 1.2, 1], opacity: [0.2, 0.3, 0.2] }}
        transition={{ duration: 8, repeat: Infinity }}
      />
      <motion.div
        className="absolute bottom-10 right-10 w-80 h-80 bg-white/10 rounded-full blur-3xl"
        animate={{ scale: [1.2, 1, 1.2], opacity: [0.2, 0.3, 0.2] }}
        transition={{ duration: 10, repeat: Infinity }}
      />

      <div className="relative container mx-auto px-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="max-w-4xl mx-auto text-center"
        >
          {/* Badge */}
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-white/10 backdrop-blur border border-white/20 rounded-full mb-8">
            <Shield className="w-4 h-4 text-white" />
            <span className="text-white/90 text-sm font-medium">
              Join 50+ Organizations Already Using Shahin-AI
            </span>
          </div>

          {/* Headline */}
          <h2 className="text-3xl md:text-5xl font-bold text-white mb-4">
            Ready to Transform Your Compliance Journey?
          </h2>
          
          {/* Arabic Headline */}
          <p className="text-2xl md:text-3xl text-white/80 mb-6" dir="rtl">
            هل أنت مستعد لتحويل رحلة الامتثال الخاصة بك؟
          </p>

          {/* Description */}
          <p className="text-lg text-white/80 mb-10 max-w-2xl mx-auto">
            Get audit-ready in 60-90 days. Reduce evidence collection time by 80%. 
            Stop chasing people and start proving compliance.
          </p>

          {/* CTA Buttons */}
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-12">
            <a
              href="#demo"
              className="group flex items-center gap-2 px-8 py-4 bg-white text-emerald-700 font-semibold rounded-xl hover:bg-emerald-50 transition-all shadow-lg"
            >
              <Calendar className="w-5 h-5" />
              Schedule a Demo
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </a>
            <a
              href="#video"
              className="group flex items-center gap-2 px-8 py-4 bg-white/10 backdrop-blur border border-white/30 text-white font-semibold rounded-xl hover:bg-white/20 transition-all"
            >
              <Play className="w-5 h-5" />
              Watch 2-Min Overview
            </a>
          </div>

          {/* Trust Elements */}
          <div className="flex flex-wrap items-center justify-center gap-6 text-white/60 text-sm">
            <div className="flex items-center gap-2">
              <Shield className="w-4 h-4" />
              <span>SOC 2 Compliant</span>
            </div>
            <div className="flex items-center gap-2">
              <Shield className="w-4 h-4" />
              <span>Data Residency in KSA</span>
            </div>
            <div className="flex items-center gap-2">
              <Shield className="w-4 h-4" />
              <span>Enterprise SLA</span>
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
