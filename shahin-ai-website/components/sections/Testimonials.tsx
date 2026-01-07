'use client';

import { motion } from 'framer-motion';
import { Rocket, Users, Gift, ArrowRight, Star, Zap } from 'lucide-react';

const benefits = [
  {
    icon: Gift,
    title: 'Founding Partner Pricing',
    titleAr: 'أسعار الشركاء المؤسسين',
    description: 'Lock in special rates as an early adopter',
  },
  {
    icon: Zap,
    title: 'Priority Support',
    titleAr: 'دعم ذو أولوية',
    description: 'Direct access to our founding team',
  },
  {
    icon: Star,
    title: 'Shape the Roadmap',
    titleAr: 'شارك في تشكيل المنتج',
    description: 'Your feedback directly influences features',
  },
  {
    icon: Users,
    title: 'Exclusive Community',
    titleAr: 'مجتمع حصري',
    description: 'Join fellow compliance leaders in KSA',
  },
];

export default function Testimonials() {
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
            <Rocket className="w-4 h-4" />
            Early Adopter Program
          </div>
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            Be Among the First
          </h2>
          <p className="text-lg text-slate-600 max-w-2xl mx-auto mb-4">
            Join our founding partners and help shape the future of GRC in Saudi Arabia.
          </p>
          <p className="text-lg text-slate-500" dir="rtl">
            انضم إلى شركائنا المؤسسين وساهم في تشكيل مستقبل الحوكمة والامتثال في المملكة
          </p>
        </motion.div>

        {/* Benefits Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {benefits.map((benefit, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.1 }}
              className="p-6 bg-white rounded-2xl border border-slate-200 hover:border-emerald-300 hover:shadow-lg transition-all text-center"
            >
              <div className="w-14 h-14 mx-auto mb-4 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-500 flex items-center justify-center">
                <benefit.icon className="w-7 h-7 text-white" />
              </div>
              <h3 className="font-bold text-slate-900 mb-1">{benefit.title}</h3>
              <p className="text-sm text-emerald-600 mb-2" dir="rtl">{benefit.titleAr}</p>
              <p className="text-sm text-slate-600">{benefit.description}</p>
            </motion.div>
          ))}
        </div>

        {/* CTA Box */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="max-w-3xl mx-auto"
        >
          <div className="bg-gradient-to-r from-emerald-600 to-teal-600 rounded-2xl p-8 text-center text-white">
            <h3 className="text-2xl font-bold mb-4">
              Limited Spots for Founding Partners
            </h3>
            <p className="text-emerald-100 mb-6">
              We're onboarding a select group of organizations who want to pioneer 
              AI-powered compliance in Saudi Arabia. Secure your spot today.
            </p>
            <a
              href="#contact"
              className="inline-flex items-center gap-2 px-8 py-4 bg-white text-emerald-700 font-semibold rounded-xl hover:bg-emerald-50 transition-all"
            >
              Apply for Early Access
              <ArrowRight className="w-5 h-5" />
            </a>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
