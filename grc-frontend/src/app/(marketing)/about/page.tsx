"use client"

// Force dynamic rendering for pages using client providers
export const dynamic = 'force-dynamic'

import { motion } from "framer-motion"
import { Target, Eye, Heart, Users, Award, Globe, ArrowLeft } from "lucide-react"
import Link from "next/link"
import { Navbar } from "@/components/layout/Navbar"
import { Footer } from "@/components/layout/Footer"
import { Button } from "@/components/ui/button"

const values = [
  {
    icon: Target,
    title: "التميز",
    description: "نسعى دائماً لتقديم أفضل الحلول وأكثرها ابتكاراً في مجال الحوكمة والامتثال",
  },
  {
    icon: Heart,
    title: "الشغف",
    description: "نحن شغوفون بمساعدة المؤسسات على تحقيق أهدافها التنظيمية بكفاءة",
  },
  {
    icon: Users,
    title: "الشراكة",
    description: "نؤمن بأن نجاح عملائنا هو نجاحنا، لذا نعمل معهم كشركاء حقيقيين",
  },
  {
    icon: Globe,
    title: "الابتكار",
    description: "نوظف أحدث التقنيات والذكاء الاصطناعي لتقديم حلول مستقبلية",
  },
]

const milestones = [
  { year: "2022", title: "التأسيس", description: "انطلاق منصة شاهين في الرياض" },
  { year: "2023", title: "التوسع", description: "دعم 50+ إطار تنظيمي" },
  { year: "2024", title: "الذكاء الاصطناعي", description: "إطلاق 9 وكلاء ذكاء اصطناعي" },
  { year: "2025", title: "الريادة", description: "500+ عميل نشط في المنطقة" },
]

const team = [
  { name: "م. أحمد الدغان", role: "المؤسس والرئيس التنفيذي", image: null },
  { name: "د. سارة العتيبي", role: "مديرة المنتج", image: null },
  { name: "م. خالد القحطاني", role: "مدير التقنية", image: null },
  { name: "أ. نورة الشمري", role: "مديرة نجاح العملاء", image: null },
]

export default function AboutPage() {
  return (
    <main className="min-h-screen bg-white dark:bg-gray-950">
      <Navbar />

      {/* Hero */}
      <section className="pt-32 pb-16 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
        <div className="container mx-auto px-6 text-center">
          <motion.span
            className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
          >
            عن شاهين
          </motion.span>
          <motion.h1
            className="text-4xl md:text-5xl font-bold text-gray-900 dark:text-white mb-6"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
          >
            نبني مستقبل الامتثال في المنطقة
          </motion.h1>
          <motion.p
            className="text-xl text-gray-600 dark:text-gray-400 max-w-3xl mx-auto"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
          >
            شاهين هي منصة سعودية رائدة في مجال إدارة الحوكمة والمخاطر والامتثال،
            نوظف أحدث تقنيات الذكاء الاصطناعي لمساعدة المؤسسات على تحقيق الامتثال بكفاءة
          </motion.p>
        </div>
      </section>

      {/* Mission & Vision */}
      <section className="py-20">
        <div className="container mx-auto px-6">
          <div className="grid md:grid-cols-2 gap-12 max-w-5xl mx-auto">
            <motion.div
              className="bg-gradient-to-br from-emerald-500 to-teal-600 rounded-2xl p-8 text-white"
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
            >
              <Target className="w-12 h-12 mb-6" />
              <h2 className="text-2xl font-bold mb-4">رسالتنا</h2>
              <p className="text-emerald-100 leading-relaxed">
                تمكين المؤسسات في المملكة العربية السعودية والمنطقة من تحقيق
                الامتثال التنظيمي بكفاءة عالية وتكلفة منخفضة من خلال منصة ذكية
                ومتكاملة تعتمد على أحدث تقنيات الذكاء الاصطناعي.
              </p>
            </motion.div>

            <motion.div
              className="bg-gray-900 dark:bg-gray-800 rounded-2xl p-8 text-white"
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
            >
              <Eye className="w-12 h-12 mb-6" />
              <h2 className="text-2xl font-bold mb-4">رؤيتنا</h2>
              <p className="text-gray-300 leading-relaxed">
                أن نكون المنصة الرائدة عالمياً في مجال إدارة الحوكمة والمخاطر
                والامتثال، ونساهم في بناء بيئة أعمال أكثر شفافية وأماناً
                تدعم رؤية المملكة 2030.
              </p>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Values */}
      <section className="py-20 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              قيمنا
            </h2>
            <p className="text-gray-600 dark:text-gray-400">
              المبادئ التي توجه عملنا كل يوم
            </p>
          </motion.div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8 max-w-6xl mx-auto">
            {values.map((value, index) => (
              <motion.div
                key={value.title}
                className="bg-white dark:bg-gray-800 rounded-2xl p-6 text-center border border-gray-200 dark:border-gray-700"
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                <div className="w-14 h-14 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center mx-auto mb-4">
                  <value.icon className="w-7 h-7 text-emerald-600 dark:text-emerald-400" />
                </div>
                <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">
                  {value.title}
                </h3>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  {value.description}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Milestones */}
      <section className="py-20">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              رحلتنا
            </h2>
            <p className="text-gray-600 dark:text-gray-400">
              محطات رئيسية في مسيرة شاهين
            </p>
          </motion.div>

          <div className="max-w-4xl mx-auto">
            <div className="relative">
              {/* Timeline Line */}
              <div className="absolute right-1/2 top-0 bottom-0 w-1 bg-gradient-to-b from-emerald-500 to-teal-600 transform translate-x-1/2 hidden md:block" />

              <div className="space-y-12">
                {milestones.map((milestone, index) => (
                  <motion.div
                    key={milestone.year}
                    className={`flex items-center gap-8 ${
                      index % 2 === 0 ? "md:flex-row" : "md:flex-row-reverse"
                    }`}
                    initial={{ opacity: 0, x: index % 2 === 0 ? -30 : 30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                  >
                    <div className={`flex-1 ${index % 2 === 0 ? "text-left" : "text-right"}`}>
                      <div className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-gray-200 dark:border-gray-700 shadow-lg">
                        <span className="text-3xl font-bold text-emerald-600">
                          {milestone.year}
                        </span>
                        <h3 className="text-xl font-bold text-gray-900 dark:text-white mt-2 mb-2">
                          {milestone.title}
                        </h3>
                        <p className="text-gray-600 dark:text-gray-400">
                          {milestone.description}
                        </p>
                      </div>
                    </div>
                    <div className="hidden md:flex w-6 h-6 rounded-full bg-emerald-500 border-4 border-white dark:border-gray-900 shadow z-10" />
                    <div className="flex-1 hidden md:block" />
                  </motion.div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Team */}
      <section className="py-20 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-6">
          <motion.div
            className="text-center mb-16"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              فريق القيادة
            </h2>
            <p className="text-gray-600 dark:text-gray-400">
              خبراء في الحوكمة والتقنية يعملون لنجاحك
            </p>
          </motion.div>

          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8 max-w-6xl mx-auto">
            {team.map((member, index) => (
              <motion.div
                key={member.name}
                className="text-center"
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                <div className="w-32 h-32 rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center mx-auto mb-4 text-white text-4xl font-bold">
                  {member.name.charAt(member.name.indexOf(" ") + 1)}
                </div>
                <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                  {member.name}
                </h3>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  {member.role}
                </p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Awards */}
      <section className="py-20">
        <div className="container mx-auto px-6 text-center">
          <motion.div
            className="max-w-4xl mx-auto"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <Award className="w-16 h-16 text-emerald-500 mx-auto mb-6" />
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              معترف بها من أفضل الجهات
            </h2>
            <p className="text-gray-600 dark:text-gray-400 mb-8">
              حاصلون على اعتمادات وشهادات من جهات محلية ودولية
            </p>
            <div className="flex flex-wrap justify-center gap-6">
              {["ISO 27001", "SOC 2 Type II", "NCA Certified", "Microsoft Partner"].map(
                (cert) => (
                  <div
                    key={cert}
                    className="px-6 py-3 bg-gray-100 dark:bg-gray-800 rounded-full text-gray-700 dark:text-gray-300 font-medium"
                  >
                    {cert}
                  </div>
                )
              )}
            </div>
          </motion.div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-20 bg-gradient-to-r from-emerald-600 to-teal-600">
        <div className="container mx-auto px-6 text-center">
          <motion.div
            className="max-w-2xl mx-auto"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-3xl font-bold text-white mb-4">
              انضم إلى رحلة الامتثال معنا
            </h2>
            <p className="text-emerald-100 mb-8">
              اكتشف كيف يمكن لمنصة شاهين مساعدتك في تحقيق أهدافك التنظيمية
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link href="/trial">
                <Button size="xl" className="bg-white text-emerald-600 hover:bg-gray-100 group">
                  ابدأ التجربة المجانية
                  <ArrowLeft className="w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                </Button>
              </Link>
              <Link href="/contact">
                <Button
                  size="xl"
                  variant="outline"
                  className="border-white text-white hover:bg-white/10"
                >
                  تواصل معنا
                </Button>
              </Link>
            </div>
          </motion.div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
