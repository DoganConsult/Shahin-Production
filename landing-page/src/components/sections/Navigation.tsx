"use client"

import { useState, useEffect } from "react"
import { motion, AnimatePresence } from "framer-motion"
import { Menu, X, Sun, Moon, LogIn, UserPlus } from "lucide-react"
import Link from "next/link"
import { useLocale, useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

// API URL for login/register - uses environment variable or defaults to localhost
const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000"
const LOGIN_URL = `${API_URL}/Account/Login`
const REGISTER_URL = `${API_URL}/Account/Register`

export function Navigation() {
  const t = useTranslations('nav')
  const locale = useLocale()
  const isRTL = locale === 'ar'
  const otherLocale = locale === 'ar' ? 'en' : 'ar'

  const [isScrolled, setIsScrolled] = useState(false)
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)
  const [isDark, setIsDark] = useState(false)
  const [scrollProgress, setScrollProgress] = useState(0)

  const navLinks = [
    { href: "#expertise", label: t('expertise') },
    { href: "#approach", label: t('approach') },
    { href: "#sectors", label: t('sectors') },
    { href: `/${locale}/contact`, label: t('contact') },
  ]

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 50)
      const docHeight = document.documentElement.scrollHeight - window.innerHeight
      const progress = (window.scrollY / docHeight) * 100
      setScrollProgress(progress)
    }

    setIsDark(document.documentElement.classList.contains('dark'))
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const toggleTheme = () => {
    const html = document.documentElement
    html.classList.toggle('dark')
    const newIsDark = html.classList.contains('dark')
    setIsDark(newIsDark)
    localStorage.setItem('theme', newIsDark ? 'dark' : 'light')
  }

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen)
    document.body.style.overflow = !isMobileMenuOpen ? 'hidden' : ''
  }

  const closeMobileMenu = () => {
    setIsMobileMenuOpen(false)
    document.body.style.overflow = ''
  }

  return (
    <>
      <motion.div
        className={cn(
          "fixed top-0 h-[3px] bg-gradient-to-r from-accent-500 via-success-500 to-accent-400 z-[1001]",
          isRTL ? "right-0" : "left-0"
        )}
        style={{ width: `${scrollProgress}%` }}
      />

      <motion.header
        className={cn(
          "fixed top-0 left-0 right-0 z-50 transition-all duration-300",
          isScrolled ? "py-3 glass shadow-lg" : "py-5 bg-transparent"
        )}
        initial={{ y: -100 }}
        animate={{ y: 0 }}
        transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
      >
        <div className="container mx-auto px-6">
          <div className="flex items-center justify-between">
            <Link href={`/${locale}`} className="relative z-10">
              <motion.div className="text-2xl font-extrabold tracking-tight" whileHover={{ scale: 1.02 }}>
                <span className={cn(
                  "transition-colors duration-300",
                  isScrolled ? "text-primary-800 dark:text-white" : "text-white"
                )}>
                  {isRTL ? "شاهين" : "SHAHIN"}
                </span>
                <span className="text-gradient">{isRTL ? " الذكاء" : " AI"}</span>
              </motion.div>
            </Link>

            <nav className="hidden md:flex items-center gap-8">
              {navLinks.map((link) => (
                <Link
                  key={link.href}
                  href={link.href}
                  className={cn(
                    "relative text-sm font-medium transition-colors duration-300 group",
                    isScrolled
                      ? "text-gray-700 dark:text-gray-200 hover:text-accent-500"
                      : "text-white/90 hover:text-white"
                  )}
                >
                  {link.label}
                  <span className={cn(
                    "absolute -bottom-1 h-0.5 bg-gradient-to-r from-accent-500 to-accent-400 transition-all duration-300 group-hover:w-full",
                    isRTL ? "right-0 w-0" : "left-0 w-0"
                  )} />
                </Link>
              ))}

              <Link
                href={`/${otherLocale}`}
                className={cn(
                  "text-sm font-medium transition-colors duration-300",
                  isScrolled
                    ? "text-gray-700 dark:text-gray-200 hover:text-accent-500"
                    : "text-white/90 hover:text-white"
                )}
              >
                {t('language')}
              </Link>

              <motion.button
                onClick={toggleTheme}
                className={cn(
                  "w-10 h-10 rounded-xl flex items-center justify-center transition-all duration-300",
                  isScrolled
                    ? "bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700"
                    : "bg-white/10 hover:bg-white/20"
                )}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                aria-label="Toggle theme"
              >
                <AnimatePresence mode="wait">
                  {isDark ? (
                    <motion.div key="sun" initial={{ rotate: -90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 90, opacity: 0 }} transition={{ duration: 0.2 }}>
                      <Sun className={cn("w-5 h-5", isScrolled ? "text-accent-500" : "text-white")} />
                    </motion.div>
                  ) : (
                    <motion.div key="moon" initial={{ rotate: 90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: -90, opacity: 0 }} transition={{ duration: 0.2 }}>
                      <Moon className={cn("w-5 h-5", isScrolled ? "text-primary-800 dark:text-white" : "text-white")} />
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.button>

              {/* Login to App Button */}
              <motion.a
                href={LOGIN_URL}
                target="_blank"
                rel="noopener noreferrer"
                className={cn(
                  "flex items-center gap-2 px-4 py-2 rounded-xl font-medium text-sm transition-all duration-300",
                  isScrolled
                    ? "bg-gradient-to-r from-accent-500 to-accent-400 text-primary-900 hover:shadow-lg hover:shadow-accent-500/30"
                    : "bg-white text-primary-900 hover:bg-accent-400"
                )}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
              >
                <LogIn className="w-4 h-4" />
                {isRTL ? "تسجيل الدخول" : "Login"}
              </motion.a>
            </nav>

            <motion.button
              onClick={toggleMobileMenu}
              className={cn(
                "md:hidden w-10 h-10 rounded-xl flex items-center justify-center z-50",
                isScrolled || isMobileMenuOpen ? "bg-gray-100 dark:bg-gray-800" : "bg-white/10"
              )}
              whileTap={{ scale: 0.95 }}
              aria-label="Toggle menu"
            >
              <AnimatePresence mode="wait">
                {isMobileMenuOpen ? (
                  <motion.div key="close" initial={{ rotate: -90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 90, opacity: 0 }}>
                    <X className="w-5 h-5 text-gray-900 dark:text-white" />
                  </motion.div>
                ) : (
                  <motion.div key="menu" initial={{ rotate: 90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: -90, opacity: 0 }}>
                    <Menu className={cn("w-5 h-5", isScrolled ? "text-gray-900 dark:text-white" : "text-white")} />
                  </motion.div>
                )}
              </AnimatePresence>
            </motion.button>
          </div>
        </div>
      </motion.header>

      <AnimatePresence>
        {isMobileMenuOpen && (
          <motion.div className="fixed inset-0 z-40 md:hidden" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
            <motion.div className="absolute inset-0 bg-primary-900/95 backdrop-blur-xl" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} onClick={closeMobileMenu} />
            <motion.nav className="absolute inset-0 flex flex-col items-center justify-center gap-8" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: 20 }} transition={{ delay: 0.1 }}>
              {navLinks.map((link, index) => (
                <motion.div key={link.href} initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 + index * 0.05 }}>
                  <Link href={link.href} onClick={closeMobileMenu} className="text-3xl font-bold text-white hover:text-accent-400 transition-colors">
                    {link.label}
                  </Link>
                </motion.div>
              ))}
              <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
                <Link href={`/${otherLocale}`} onClick={closeMobileMenu} className="text-2xl font-semibold text-white/80 hover:text-accent-400 transition-colors">
                  {t('language')}
                </Link>
              </motion.div>

              {/* Login Button - Mobile */}
              <motion.a
                href={LOGIN_URL}
                target="_blank"
                rel="noopener noreferrer"
                className="mt-6 flex items-center gap-3 px-8 py-4 rounded-2xl bg-gradient-to-r from-accent-500 to-accent-400 text-primary-900 font-bold text-xl"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.35 }}
                whileTap={{ scale: 0.95 }}
                onClick={closeMobileMenu}
              >
                <LogIn className="w-6 h-6" />
                {isRTL ? "تسجيل الدخول" : "Login to App"}
              </motion.a>

              <motion.button onClick={toggleTheme} className="mt-4 w-14 h-14 rounded-2xl bg-white/10 flex items-center justify-center" initial={{ opacity: 0, scale: 0.8 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: 0.4 }} whileTap={{ scale: 0.95 }}>
                {isDark ? <Sun className="w-6 h-6 text-accent-400" /> : <Moon className="w-6 h-6 text-white" />}
              </motion.button>
            </motion.nav>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  )
}
