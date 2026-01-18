"use client"

import { useState } from "react"
import { motion } from "framer-motion"
import { Linkedin, Twitter, Mail, Send, ArrowUp, CheckCircle2, AlertCircle, Loader2 } from "lucide-react"
import Link from "next/link"
import { useLocale, useTranslations } from "next-intl"
import { cn } from "@/lib/utils"
import { subscribeNewsletter } from "@/lib/api"

const socialLinks = [
  { icon: Linkedin, href: "#", label: "LinkedIn" },
  { icon: Twitter, href: "#", label: "Twitter" },
  { icon: Mail, href: "mailto:info@doganconsult.com", label: "Email" },
]

export function Footer() {
  const t = useTranslations('footer')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const [email, setEmail] = useState("")
  const [isScrollVisible, setIsScrollVisible] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubscribed, setIsSubscribed] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Check scroll position for back-to-top button
  if (typeof window !== 'undefined') {
    window.addEventListener('scroll', () => {
      setIsScrollVisible(window.scrollY > 500)
    })
  }

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const handleNewsletterSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!email.trim() || isSubmitting) return

    setIsSubmitting(true)
    setError(null)

    try {
      const response = await subscribeNewsletter({
        email: email.trim(),
        locale
      })

      if (response.success) {
        setIsSubscribed(true)
        setEmail("")
        // Reset success message after 5 seconds
        setTimeout(() => setIsSubscribed(false), 5000)
      } else {
        setError(response.message || (locale === 'ar'
          ? 'فشل الاشتراك. حاول مرة أخرى.'
          : 'Failed to subscribe. Please try again.'))
      }
    } catch (err) {
      console.error('Newsletter subscription error:', err)
      setError(locale === 'ar'
        ? 'حدث خطأ أثناء الاشتراك. حاول مرة أخرى.'
        : 'An error occurred. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const serviceLinks = [
    { labelKey: 'services.telecommunications', href: `/${locale}/services/telecommunications` },
    { labelKey: 'services.dataCenters', href: `/${locale}/services/data-centers` },
    { labelKey: 'services.cybersecurity', href: `/${locale}/services/cybersecurity` },
    { labelKey: 'services.governance', href: `/${locale}/services/governance` },
  ]

  const companyLinks = [
    { labelKey: 'company.about', href: `/${locale}/about` },
    { labelKey: 'company.contact', href: `/${locale}/contact` },
    { labelKey: 'company.language', href: locale === 'ar' ? '/en' : '/ar' },
  ]

  return (
    <>
      <footer className="bg-gray-950 text-white pt-20 pb-8" dir={isRTL ? 'rtl' : 'ltr'}>
        <div className="container mx-auto px-6">
          {/* Main Footer Grid */}
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-12 mb-16">
            {/* Brand Column */}
            <div className="lg:col-span-1">
              <Link href={`/${locale}`} className="inline-block mb-6">
                <span className="text-2xl font-extrabold">
                  DOGAN<span className="text-gradient">CONSULT</span>
                </span>
              </Link>
              <p className="text-gray-400 text-sm leading-relaxed mb-6">
                {t('description')}
              </p>

              {/* Social Links */}
              <div className="flex gap-3">
                {socialLinks.map((social) => (
                  <motion.a
                    key={social.label}
                    href={social.href}
                    className={cn(
                      "w-10 h-10 rounded-xl bg-white/5 flex items-center justify-center",
                      "transition-all duration-300",
                      "hover:bg-accent-500 hover:text-primary-900"
                    )}
                    whileHover={{ scale: 1.1, y: -3 }}
                    whileTap={{ scale: 0.95 }}
                    aria-label={social.label}
                  >
                    <social.icon className="w-5 h-5" />
                  </motion.a>
                ))}
              </div>
            </div>

            {/* Services Links */}
            <div>
              <h4 className="text-sm font-bold uppercase tracking-wider mb-6">{t('servicesTitle')}</h4>
              <ul className="space-y-3">
                {serviceLinks.map((link) => (
                  <li key={link.labelKey}>
                    <Link
                      href={link.href}
                      className={cn(
                        "text-gray-400 text-sm hover:text-accent-400 transition-colors inline-flex items-center gap-2 group",
                        isRTL && "flex-row-reverse"
                      )}
                    >
                      <span className={cn(
                        "w-0 h-0.5 bg-accent-500 transition-all duration-300 group-hover:w-3"
                      )} />
                      {t(link.labelKey)}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Company Links */}
            <div>
              <h4 className="text-sm font-bold uppercase tracking-wider mb-6">{t('companyTitle')}</h4>
              <ul className="space-y-3">
                {companyLinks.map((link) => (
                  <li key={link.labelKey}>
                    <Link
                      href={link.href}
                      className={cn(
                        "text-gray-400 text-sm hover:text-accent-400 transition-colors inline-flex items-center gap-2 group",
                        isRTL && "flex-row-reverse"
                      )}
                    >
                      <span className={cn(
                        "w-0 h-0.5 bg-accent-500 transition-all duration-300 group-hover:w-3"
                      )} />
                      {t(link.labelKey)}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Newsletter */}
            <div>
              <h4 className="text-sm font-bold uppercase tracking-wider mb-6">{t('newsletter.title')}</h4>
              <p className="text-gray-400 text-sm mb-4">
                {t('newsletter.description')}
              </p>

              {isSubscribed ? (
                <motion.div
                  className="flex items-center gap-2 p-3 rounded-xl bg-green-500/10 border border-green-500/30"
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                >
                  <CheckCircle2 className="w-5 h-5 text-green-400 flex-shrink-0" />
                  <p className="text-sm text-green-400">
                    {locale === 'ar' ? 'تم الاشتراك بنجاح!' : 'Successfully subscribed!'}
                  </p>
                </motion.div>
              ) : (
                <div className="space-y-2">
                  <form onSubmit={handleNewsletterSubmit} className="flex gap-2">
                    <input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder={t('newsletter.placeholder')}
                      disabled={isSubmitting}
                      className={cn(
                        "flex-1 px-4 py-3 rounded-xl text-sm",
                        "bg-white/5 border border-white/10",
                        "placeholder:text-gray-500",
                        "focus:outline-none focus:border-accent-500 focus:bg-white/10",
                        "transition-all duration-300",
                        "disabled:opacity-50 disabled:cursor-not-allowed"
                      )}
                    />
                    <motion.button
                      type="submit"
                      disabled={isSubmitting || !email.trim()}
                      className={cn(
                        "px-4 py-3 rounded-xl",
                        "bg-gradient-to-r from-accent-500 to-accent-400",
                        "text-primary-900 font-semibold",
                        "hover:shadow-lg hover:shadow-accent-500/30",
                        "transition-all duration-300",
                        "disabled:opacity-50 disabled:cursor-not-allowed"
                      )}
                      whileHover={!isSubmitting ? { scale: 1.05 } : {}}
                      whileTap={!isSubmitting ? { scale: 0.95 } : {}}
                    >
                      {isSubmitting ? (
                        <Loader2 className="w-5 h-5 animate-spin" />
                      ) : (
                        <Send className="w-5 h-5" />
                      )}
                    </motion.button>
                  </form>

                  {error && (
                    <motion.div
                      className="flex items-start gap-2 p-2 rounded-lg bg-red-500/10 border border-red-500/30"
                      initial={{ opacity: 0, y: -5 }}
                      animate={{ opacity: 1, y: 0 }}
                    >
                      <AlertCircle className="w-4 h-4 text-red-400 flex-shrink-0 mt-0.5" />
                      <p className="text-xs text-red-400">{error}</p>
                    </motion.div>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Bottom Bar */}
          <div className="pt-8 border-t border-white/10 flex flex-col md:flex-row justify-between items-center gap-4">
            <p className="text-gray-500 text-sm">
              {t('copyright', { year: new Date().getFullYear() })}
            </p>
            <div className="flex gap-6">
              <Link href={`/${locale}/privacy`} className="text-gray-500 text-sm hover:text-accent-400 transition-colors">
                {t('legal.privacy')}
              </Link>
              <Link href={`/${locale}/terms`} className="text-gray-500 text-sm hover:text-accent-400 transition-colors">
                {t('legal.terms')}
              </Link>
              <Link href={`/${locale}/cookies`} className="text-gray-500 text-sm hover:text-accent-400 transition-colors">
                {t('legal.cookies')}
              </Link>
            </div>
          </div>
        </div>
      </footer>

      {/* Back to Top Button */}
      <motion.button
        onClick={scrollToTop}
        className={cn(
          "fixed bottom-8 z-50 w-12 h-12 rounded-xl",
          "bg-gradient-to-r from-accent-500 to-accent-400",
          "text-primary-900 shadow-lg shadow-accent-500/30",
          "flex items-center justify-center",
          "transition-all duration-300",
          isScrollVisible ? "opacity-100 translate-y-0" : "opacity-0 translate-y-4 pointer-events-none",
          isRTL ? "left-8" : "right-8"
        )}
        whileHover={{ scale: 1.1, y: -3 }}
        whileTap={{ scale: 0.95 }}
        aria-label="Back to top"
      >
        <ArrowUp className="w-5 h-5" />
      </motion.button>
    </>
  )
}
