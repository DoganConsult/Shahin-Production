"use client"

import { useState } from "react"
import { motion } from "framer-motion"
import { Calculator, TrendingUp, Clock, DollarSign, ArrowRight } from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useLocale } from "@/components/providers/locale-provider"

const content = {
  sectionLabel: { en: "ROI Calculator", ar: "حاسبة العائد على الاستثمار" },
  title: { en: "Calculate Your Savings", ar: "احسب مدخراتك" },
  subtitle: {
    en: "See how much time and money Shahin can save your organization on compliance activities.",
    ar: "اكتشف كم من الوقت والمال يمكن لشاهين توفيره لمؤسستك في أنشطة الامتثال."
  },
  inputPanel: {
    title: { en: "Your Organization", ar: "مؤسستك" },
    teamSize: { en: "Compliance Team Size", ar: "حجم فريق الامتثال" },
    people: { en: "people", ar: "شخص" },
    assessmentsPerYear: { en: "Compliance Assessments Per Year", ar: "تقييمات الامتثال سنوياً" },
    hoursPerAssessment: { en: "Hours Per Assessment (per person)", ar: "ساعات لكل تقييم (للشخص الواحد)" },
    hrs: { en: "hrs", ar: "ساعة" }
  },
  resultsPanel: {
    title: { en: "Your Estimated Savings", ar: "مدخراتك المقدرة" },
    annualCostSavings: { en: "Annual Cost Savings", ar: "التوفير السنوي" },
    sar: { en: "SAR", ar: "ريال" },
    timeSaved: { en: "Time Saved Annually", ar: "الوقت الموفر سنوياً" },
    hours: { en: "hours", ar: "ساعة" },
    workWeeks: { en: "That's {weeks} work weeks!", ar: "هذا يعادل {weeks} أسابيع عمل!" },
    timeReduction: { en: "Time Reduction", ar: "تقليل الوقت" },
    accuracyRate: { en: "Accuracy Rate", ar: "معدل الدقة" },
    cta: { en: "Start Saving Today", ar: "ابدأ التوفير اليوم" }
  },
  disclaimer: {
    en: "* Estimates based on industry averages. Actual savings may vary based on your organization's specific circumstances.",
    ar: "* التقديرات مبنية على متوسطات الصناعة. قد تختلف المدخرات الفعلية بناءً على ظروف مؤسستك الخاصة."
  }
}

export function ROICalculator() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [teamSize, setTeamSize] = useState(10)
  const [assessmentsPerYear, setAssessmentsPerYear] = useState(4)
  const [hoursPerAssessment, setHoursPerAssessment] = useState(40)

  // Calculate savings
  const hourlyRate = 150 // SAR per hour for compliance professional
  const timeSavingsPercent = 0.7 // 70% time savings
  const manualHours = teamSize * assessmentsPerYear * hoursPerAssessment
  const automatedHours = manualHours * (1 - timeSavingsPercent)
  const hoursSaved = manualHours - automatedHours
  const costSavings = hoursSaved * hourlyRate
  const weeksSaved = Math.round(hoursSaved / 40)

  return (
    <section className="py-24 bg-gradient-to-b from-white to-gray-50 dark:from-gray-950 dark:to-gray-900">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block">
            {isArabic ? content.sectionLabel.ar : content.sectionLabel.en}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? content.title.ar : content.title.en}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {isArabic ? content.subtitle.ar : content.subtitle.en}
          </p>
        </motion.div>

        <div className="max-w-5xl mx-auto">
          <motion.div
            className="grid lg:grid-cols-2 gap-8"
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            {/* Input Panel */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl p-8 border border-gray-200 dark:border-gray-700 shadow-lg">
              <div className="flex items-center gap-3 mb-8">
                <div className="w-12 h-12 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                  <Calculator className="w-6 h-6 text-emerald-600 dark:text-emerald-400" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                  {isArabic ? content.inputPanel.title.ar : content.inputPanel.title.en}
                </h3>
              </div>

              <div className="space-y-8">
                {/* Team Size */}
                <div>
                  <div className="flex justify-between mb-2">
                    <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                      {isArabic ? content.inputPanel.teamSize.ar : content.inputPanel.teamSize.en}
                    </label>
                    <span className="text-sm font-bold text-emerald-600 dark:text-emerald-400">
                      {teamSize} {isArabic ? content.inputPanel.people.ar : content.inputPanel.people.en}
                    </span>
                  </div>
                  <input
                    type="range"
                    min="1"
                    max="50"
                    value={teamSize}
                    onChange={(e) => setTeamSize(Number(e.target.value))}
                    className="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer accent-emerald-500"
                  />
                  <div className="flex justify-between text-xs text-gray-500 mt-1">
                    <span>1</span>
                    <span>50</span>
                  </div>
                </div>

                {/* Assessments Per Year */}
                <div>
                  <div className="flex justify-between mb-2">
                    <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                      {isArabic ? content.inputPanel.assessmentsPerYear.ar : content.inputPanel.assessmentsPerYear.en}
                    </label>
                    <span className="text-sm font-bold text-emerald-600 dark:text-emerald-400">
                      {assessmentsPerYear}
                    </span>
                  </div>
                  <input
                    type="range"
                    min="1"
                    max="12"
                    value={assessmentsPerYear}
                    onChange={(e) => setAssessmentsPerYear(Number(e.target.value))}
                    className="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer accent-emerald-500"
                  />
                  <div className="flex justify-between text-xs text-gray-500 mt-1">
                    <span>1</span>
                    <span>12</span>
                  </div>
                </div>

                {/* Hours Per Assessment */}
                <div>
                  <div className="flex justify-between mb-2">
                    <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                      {isArabic ? content.inputPanel.hoursPerAssessment.ar : content.inputPanel.hoursPerAssessment.en}
                    </label>
                    <span className="text-sm font-bold text-emerald-600 dark:text-emerald-400">
                      {hoursPerAssessment} {isArabic ? content.inputPanel.hrs.ar : content.inputPanel.hrs.en}
                    </span>
                  </div>
                  <input
                    type="range"
                    min="10"
                    max="100"
                    step="5"
                    value={hoursPerAssessment}
                    onChange={(e) => setHoursPerAssessment(Number(e.target.value))}
                    className="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-lg appearance-none cursor-pointer accent-emerald-500"
                  />
                  <div className="flex justify-between text-xs text-gray-500 mt-1">
                    <span>10 {isArabic ? content.inputPanel.hrs.ar : content.inputPanel.hrs.en}</span>
                    <span>100 {isArabic ? content.inputPanel.hrs.ar : content.inputPanel.hrs.en}</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Results Panel */}
            <div className="bg-gradient-to-br from-emerald-500 to-teal-600 rounded-2xl p-8 text-white shadow-lg">
              <div className="flex items-center gap-3 mb-8">
                <div className="w-12 h-12 rounded-xl bg-white/20 flex items-center justify-center">
                  <TrendingUp className="w-6 h-6 text-white" />
                </div>
                <h3 className="text-xl font-bold">
                  {isArabic ? content.resultsPanel.title.ar : content.resultsPanel.title.en}
                </h3>
              </div>

              <div className="space-y-6">
                {/* Annual Cost Savings */}
                <div className="bg-white/10 backdrop-blur rounded-xl p-6">
                  <div className="flex items-center gap-3 mb-2">
                    <DollarSign className="w-5 h-5 text-emerald-200" />
                    <span className="text-emerald-100 text-sm">
                      {isArabic ? content.resultsPanel.annualCostSavings.ar : content.resultsPanel.annualCostSavings.en}
                    </span>
                  </div>
                  <div className="text-4xl font-bold">
                    {costSavings.toLocaleString()} {isArabic ? content.resultsPanel.sar.ar : content.resultsPanel.sar.en}
                  </div>
                </div>

                {/* Time Saved */}
                <div className="bg-white/10 backdrop-blur rounded-xl p-6">
                  <div className="flex items-center gap-3 mb-2">
                    <Clock className="w-5 h-5 text-emerald-200" />
                    <span className="text-emerald-100 text-sm">
                      {isArabic ? content.resultsPanel.timeSaved.ar : content.resultsPanel.timeSaved.en}
                    </span>
                  </div>
                  <div className="text-4xl font-bold">
                    {hoursSaved.toLocaleString()} {isArabic ? content.resultsPanel.hours.ar : content.resultsPanel.hours.en}
                  </div>
                  <div className="text-emerald-200 text-sm mt-1">
                    {isArabic
                      ? content.resultsPanel.workWeeks.ar.replace("{weeks}", String(weeksSaved))
                      : content.resultsPanel.workWeeks.en.replace("{weeks}", String(weeksSaved))}
                  </div>
                </div>

                {/* Efficiency Gain */}
                <div className="grid grid-cols-2 gap-4">
                  <div className="bg-white/10 backdrop-blur rounded-xl p-4 text-center">
                    <div className="text-3xl font-bold">70%</div>
                    <div className="text-emerald-200 text-xs">
                      {isArabic ? content.resultsPanel.timeReduction.ar : content.resultsPanel.timeReduction.en}
                    </div>
                  </div>
                  <div className="bg-white/10 backdrop-blur rounded-xl p-4 text-center">
                    <div className="text-3xl font-bold">99%</div>
                    <div className="text-emerald-200 text-xs">
                      {isArabic ? content.resultsPanel.accuracyRate.ar : content.resultsPanel.accuracyRate.en}
                    </div>
                  </div>
                </div>

                {/* CTA */}
                <Link href="/trial" className="block">
                  <Button className="w-full bg-white text-emerald-700 hover:bg-gray-100 py-6 text-lg font-semibold group">
                    {isArabic ? content.resultsPanel.cta.ar : content.resultsPanel.cta.en}
                    <ArrowRight className={`w-5 h-5 ${isArabic ? "mr-2 group-hover:-translate-x-1" : "ml-2 group-hover:translate-x-1"} transition-transform`} />
                  </Button>
                </Link>
              </div>
            </div>
          </motion.div>

          {/* Disclaimer */}
          <p className="text-center text-xs text-gray-500 dark:text-gray-400 mt-6">
            {isArabic ? content.disclaimer.ar : content.disclaimer.en}
          </p>
        </div>
      </div>
    </section>
  )
}
