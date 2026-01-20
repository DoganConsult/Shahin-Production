"use client"

import { useState } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Calendar,
  Clock,
  Video,
  Check,
  ArrowRight,
  Building2,
  Users,
  Sparkles
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLocale } from "@/components/providers/locale-provider"

type QualificationData = {
  companySize: string
  hasERP: boolean
}

const companySizes = [
  { value: "small", label: "1-50 employees", labelAr: "1-50 موظف", qualified: false },
  { value: "medium", label: "51-200 employees", labelAr: "51-200 موظف", qualified: true },
  { value: "large", label: "201-500 employees", labelAr: "201-500 موظف", qualified: true },
  { value: "enterprise", label: "500+ employees", labelAr: "500+ موظف", qualified: true }
]

const timeSlots = [
  { time: "09:00", label: "9:00 AM", labelAr: "9:00 صباحاً" },
  { time: "10:00", label: "10:00 AM", labelAr: "10:00 صباحاً" },
  { time: "11:00", label: "11:00 AM", labelAr: "11:00 صباحاً" },
  { time: "14:00", label: "2:00 PM", labelAr: "2:00 مساءً" },
  { time: "15:00", label: "3:00 PM", labelAr: "3:00 مساءً" },
  { time: "16:00", label: "4:00 PM", labelAr: "4:00 مساءً" }
]

const content = {
  sectionLabel: { en: "Schedule a Demo", ar: "حجز عرض توضيحي" },
  title: { en: "See Shahin in Action", ar: "شاهد شاهين على أرض الواقع" },
  subtitle: {
    en: "Book a personalized demo with our team. We'll show you exactly how Shahin can automate your workflows.",
    ar: "احجز عرضاً توضيحياً مخصصاً مع فريقنا. سنُريك بالضبط كيف يمكن لشاهين أتمتة سير عملك."
  },
  qualification: {
    title: { en: "Quick Qualification", ar: "تأهيل سريع" },
    subtitle: { en: "Help us prepare the right demo for you", ar: "ساعدنا في إعداد العرض المناسب لك" },
    companySize: { en: "Company Size", ar: "حجم الشركة" },
    erpQuestion: { en: "Do you currently use an ERP system?", ar: "هل تستخدم حالياً نظام ERP؟" },
    yesErp: { en: "Yes, we have an ERP", ar: "نعم، لدينا ERP" },
    noErp: { en: "No, but interested", ar: "لا، ولكن مهتمون" },
    smallTeamMessage: {
      title: { en: "For smaller teams:", ar: "للفرق الصغيرة:" },
      text: { en: "We recommend starting with our self-service trial. You'll get full access to explore Shahin at your own pace.", ar: "نوصي بالبدء بالنسخة التجريبية الذاتية. ستحصل على وصول كامل لاستكشاف شاهين بالسرعة التي تناسبك." }
    },
    startTrial: { en: "Start Free Trial", ar: "ابدأ النسخة التجريبية المجانية" },
    continueBtn: { en: "Continue to Schedule", ar: "متابعة الجدولة" }
  },
  booking: {
    liveDemo: { en: "Live Demo Call", ar: "مكالمة عرض توضيحي مباشر" },
    duration: { en: "30 minutes", ar: "30 دقيقة" },
    description: { en: "Join our product team for a personalized walkthrough of Shahin's capabilities.", ar: "انضم إلى فريق المنتج لجولة مخصصة في قدرات شاهين." },
    features: {
      aiAgents: { en: "See AI agents in action", ar: "شاهد وكلاء الذكاء الاصطناعي على أرض الواقع" },
      erpIntegration: { en: "Explore ERP integrations", ar: "استكشف تكاملات ERP" },
      qa: { en: "Q&A with our team", ar: "أسئلة وأجوبة مع فريقنا" },
      customUseCase: { en: "Custom use case discussion", ar: "مناقشة حالة الاستخدام المخصصة" }
    },
    selectDate: { en: "Select a Date", ar: "اختر تاريخاً" },
    selectTime: { en: "Select a Time (Saudi Arabia Time)", ar: "اختر وقتاً (توقيت السعودية)" },
    yourEmail: { en: "Your Email", ar: "بريدك الإلكتروني" },
    emailPlaceholder: { en: "you@company.com", ar: "you@company.com" },
    confirmBtn: { en: "Confirm Booking", ar: "تأكيد الحجز" }
  },
  confirmed: {
    title: { en: "You're All Set!", ar: "أنت جاهز!" },
    emailSent: { en: "We've sent a calendar invite to", ar: "لقد أرسلنا دعوة تقويم إلى" },
    at: { en: "at", ar: "في" }
  }
}

export function BookingWidget() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [step, setStep] = useState<"qualify" | "book" | "confirmed">("qualify")
  const [qualification, setQualification] = useState<QualificationData>({
    companySize: "",
    hasERP: false
  })
  const [selectedDate, setSelectedDate] = useState<string>("")
  const [selectedTime, setSelectedTime] = useState<string>("")
  const [email, setEmail] = useState("")

  // Check if user qualifies for a demo call
  const isQualified = () => {
    const sizeData = companySizes.find(s => s.value === qualification.companySize)
    return sizeData?.qualified && qualification.hasERP
  }

  // Generate next 5 weekdays
  const getAvailableDates = () => {
    const dates: { date: string; label: string }[] = []
    const today = new Date()
    let count = 0
    let daysAdded = 0

    while (daysAdded < 5) {
      const nextDay = new Date(today)
      nextDay.setDate(today.getDate() + count + 1)

      // Skip weekends
      if (nextDay.getDay() !== 0 && nextDay.getDay() !== 6) {
        dates.push({
          date: nextDay.toISOString().split('T')[0],
          label: nextDay.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })
        })
        daysAdded++
      }
      count++
    }
    return dates
  }

  const handleQualify = () => {
    if (isQualified()) {
      setStep("book")
    }
  }

  const handleBook = () => {
    if (selectedDate && selectedTime && email) {
      // In production, this would call your booking API
      console.log("Booking:", { email, date: selectedDate, time: selectedTime })
      setStep("confirmed")
    }
  }

  return (
    <section className="py-24 bg-white dark:bg-gray-950">
      <div className="container mx-auto px-6">
        <div className="max-w-4xl mx-auto">
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

          {/* Booking Card */}
          <motion.div
            className="bg-gray-50 dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden"
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <AnimatePresence mode="wait">
              {/* Step 1: Qualification */}
              {step === "qualify" && (
                <motion.div
                  key="qualify"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  className="p-8"
                >
                  <div className="flex items-center gap-3 mb-8">
                    <div className="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                      <Users className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900 dark:text-white">
                        {isArabic ? content.qualification.title.ar : content.qualification.title.en}
                      </h3>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        {isArabic ? content.qualification.subtitle.ar : content.qualification.subtitle.en}
                      </p>
                    </div>
                  </div>

                  <div className="space-y-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                        {isArabic ? content.qualification.companySize.ar : content.qualification.companySize.en}
                      </label>
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                        {companySizes.map((size) => (
                          <button
                            key={size.value}
                            type="button"
                            onClick={() => setQualification(prev => ({ ...prev, companySize: size.value }))}
                            className={`p-3 rounded-xl border text-sm font-medium transition-all ${
                              qualification.companySize === size.value
                                ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                                : "border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600"
                            }`}
                          >
                            {isArabic ? size.labelAr : size.label}
                          </button>
                        ))}
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                        {isArabic ? content.qualification.erpQuestion.ar : content.qualification.erpQuestion.en}
                      </label>
                      <div className="flex gap-3">
                        <button
                          type="button"
                          onClick={() => setQualification(prev => ({ ...prev, hasERP: true }))}
                          className={`flex-1 p-4 rounded-xl border text-sm font-medium transition-all ${
                            qualification.hasERP
                              ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                              : "border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600"
                          }`}
                        >
                          <Building2 className="w-5 h-5 mx-auto mb-2" />
                          {isArabic ? content.qualification.yesErp.ar : content.qualification.yesErp.en}
                        </button>
                        <button
                          type="button"
                          onClick={() => setQualification(prev => ({ ...prev, hasERP: false }))}
                          className={`flex-1 p-4 rounded-xl border text-sm font-medium transition-all ${
                            !qualification.hasERP && qualification.companySize
                              ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                              : "border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600"
                          }`}
                        >
                          <Sparkles className="w-5 h-5 mx-auto mb-2" />
                          {isArabic ? content.qualification.noErp.ar : content.qualification.noErp.en}
                        </button>
                      </div>
                    </div>

                    {qualification.companySize && !isQualified() && (
                      <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-xl p-4">
                        <p className="text-sm text-blue-700 dark:text-blue-400">
                          <strong>{isArabic ? content.qualification.smallTeamMessage.title.ar : content.qualification.smallTeamMessage.title.en}</strong> {isArabic ? content.qualification.smallTeamMessage.text.ar : content.qualification.smallTeamMessage.text.en}
                        </p>
                        <Button className="mt-3 bg-blue-600 hover:bg-blue-700 text-white">
                          {isArabic ? content.qualification.startTrial.ar : content.qualification.startTrial.en}
                        </Button>
                      </div>
                    )}

                    {isQualified() && (
                      <Button
                        onClick={handleQualify}
                        className="w-full bg-emerald-600 hover:bg-emerald-700 text-white py-6 text-lg font-semibold group"
                      >
                        {isArabic ? content.qualification.continueBtn.ar : content.qualification.continueBtn.en}
                        <ArrowRight className={`w-5 h-5 ${isArabic ? "mr-2 group-hover:-translate-x-1" : "ml-2 group-hover:translate-x-1"} transition-transform`} />
                      </Button>
                    )}
                  </div>
                </motion.div>
              )}

              {/* Step 2: Book Time */}
              {step === "book" && (
                <motion.div
                  key="book"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  className="grid md:grid-cols-2"
                >
                  {/* Left: Info Panel */}
                  <div className="bg-emerald-600 p-8 text-white">
                    <div className="flex items-center gap-3 mb-6">
                      <div className="w-12 h-12 rounded-xl bg-white/20 flex items-center justify-center">
                        <Video className="w-6 h-6" />
                      </div>
                      <div>
                        <h3 className="font-bold text-lg">{isArabic ? content.booking.liveDemo.ar : content.booking.liveDemo.en}</h3>
                        <p className="text-emerald-100 text-sm">{isArabic ? content.booking.duration.ar : content.booking.duration.en}</p>
                      </div>
                    </div>

                    <p className="text-emerald-100 mb-6">
                      {isArabic ? content.booking.description.ar : content.booking.description.en}
                    </p>

                    <div className="space-y-4">
                      <div className="flex items-start gap-3">
                        <Check className="w-5 h-5 text-emerald-200 mt-0.5" />
                        <span className="text-sm">{isArabic ? content.booking.features.aiAgents.ar : content.booking.features.aiAgents.en}</span>
                      </div>
                      <div className="flex items-start gap-3">
                        <Check className="w-5 h-5 text-emerald-200 mt-0.5" />
                        <span className="text-sm">{isArabic ? content.booking.features.erpIntegration.ar : content.booking.features.erpIntegration.en}</span>
                      </div>
                      <div className="flex items-start gap-3">
                        <Check className="w-5 h-5 text-emerald-200 mt-0.5" />
                        <span className="text-sm">{isArabic ? content.booking.features.qa.ar : content.booking.features.qa.en}</span>
                      </div>
                      <div className="flex items-start gap-3">
                        <Check className="w-5 h-5 text-emerald-200 mt-0.5" />
                        <span className="text-sm">{isArabic ? content.booking.features.customUseCase.ar : content.booking.features.customUseCase.en}</span>
                      </div>
                    </div>
                  </div>

                  {/* Right: Calendar */}
                  <div className="p-8">
                    <div className="mb-6">
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                        {isArabic ? content.booking.selectDate.ar : content.booking.selectDate.en}
                      </label>
                      <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                        {getAvailableDates().map((date) => (
                          <button
                            key={date.date}
                            type="button"
                            onClick={() => setSelectedDate(date.date)}
                            className={`p-3 rounded-lg border text-sm font-medium transition-all ${
                              selectedDate === date.date
                                ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                                : "border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:border-gray-300"
                            }`}
                          >
                            {date.label}
                          </button>
                        ))}
                      </div>
                    </div>

                    {selectedDate && (
                      <div className="mb-6">
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                          {isArabic ? content.booking.selectTime.ar : content.booking.selectTime.en}
                        </label>
                        <div className="grid grid-cols-3 gap-2">
                          {timeSlots.map((slot) => (
                            <button
                              key={slot.time}
                              type="button"
                              onClick={() => setSelectedTime(slot.time)}
                              className={`p-2 rounded-lg border text-sm font-medium transition-all ${
                                selectedTime === slot.time
                                  ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                                  : "border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:border-gray-300"
                              }`}
                            >
                              {isArabic ? slot.labelAr : slot.label}
                            </button>
                          ))}
                        </div>
                      </div>
                    )}

                    {selectedDate && selectedTime && (
                      <div className="mb-6">
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          {isArabic ? content.booking.yourEmail.ar : content.booking.yourEmail.en}
                        </label>
                        <input
                          type="email"
                          value={email}
                          onChange={(e) => setEmail(e.target.value)}
                          placeholder={isArabic ? content.booking.emailPlaceholder.ar : content.booking.emailPlaceholder.en}
                          className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-xl bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none"
                        />
                      </div>
                    )}

                    <Button
                      onClick={handleBook}
                      disabled={!selectedDate || !selectedTime || !email}
                      className="w-full bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-300 disabled:cursor-not-allowed text-white py-5 font-semibold"
                    >
                      <Calendar className={`w-5 h-5 ${isArabic ? "ml-2" : "mr-2"}`} />
                      {isArabic ? content.booking.confirmBtn.ar : content.booking.confirmBtn.en}
                    </Button>
                  </div>
                </motion.div>
              )}

              {/* Step 3: Confirmed */}
              {step === "confirmed" && (
                <motion.div
                  key="confirmed"
                  initial={{ opacity: 0, scale: 0.95 }}
                  animate={{ opacity: 1, scale: 1 }}
                  className="p-12 text-center"
                >
                  <div className="w-20 h-20 rounded-full bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center mx-auto mb-6">
                    <Check className="w-10 h-10 text-emerald-600" />
                  </div>
                  <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-3">
                    {isArabic ? content.confirmed.title.ar : content.confirmed.title.en}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-400 mb-6">
                    {isArabic ? content.confirmed.emailSent.ar : content.confirmed.emailSent.en} <strong>{email}</strong>
                  </p>
                  <div className="inline-flex items-center gap-3 px-6 py-3 bg-gray-100 dark:bg-gray-800 rounded-xl">
                    <Clock className="w-5 h-5 text-emerald-600" />
                    <span className="text-gray-700 dark:text-gray-300">
                      {new Date(selectedDate).toLocaleDateString(isArabic ? 'ar-SA' : 'en-US', { weekday: 'long', month: 'long', day: 'numeric' })} {isArabic ? content.confirmed.at.ar : content.confirmed.at.en} {isArabic ? timeSlots.find(s => s.time === selectedTime)?.labelAr : timeSlots.find(s => s.time === selectedTime)?.label}
                    </span>
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </motion.div>
        </div>
      </div>
    </section>
  )
}
