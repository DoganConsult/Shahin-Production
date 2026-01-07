'use client';

import { motion } from 'framer-motion';

const frameworks = [
  { name: 'NCA-ECC', nameAr: 'الضوابط الأساسية' },
  { name: 'PDPL', nameAr: 'حماية البيانات' },
  { name: 'SAMA CSF', nameAr: 'ساما' },
  { name: 'ISO 27001', nameAr: 'آيزو' },
  { name: 'SOC 2', nameAr: 'سوك' },
  { name: 'GDPR', nameAr: 'اللائحة الأوروبية' },
];

export default function TrustStrip() {
  return (
    <section className="py-8 bg-slate-50 border-y border-slate-200">
      <div className="container mx-auto px-6">
        <motion.div
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          className="text-center"
        >
          <p className="text-slate-500 text-sm mb-6">
            Compliant with Saudi and International Regulatory Frameworks
          </p>
          
          <div className="flex flex-wrap items-center justify-center gap-6 md:gap-12">
            {frameworks.map((framework, index) => (
              <motion.div
                key={framework.name}
                initial={{ opacity: 0, y: 10 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="flex flex-col items-center"
              >
                <div className="px-4 py-2 bg-white rounded-lg border border-slate-200 shadow-sm hover:shadow-md hover:border-emerald-300 transition-all">
                  <span className="font-semibold text-slate-700">{framework.name}</span>
                </div>
                <span className="text-xs text-slate-400 mt-1" dir="rtl">{framework.nameAr}</span>
              </motion.div>
            ))}
          </div>
        </motion.div>
      </div>
    </section>
  );
}
