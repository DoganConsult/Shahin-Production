"use client"

import Link from "next/link"
import { motion } from "framer-motion"
import { Mail, Phone, MapPin, Linkedin, Twitter } from "lucide-react"
import { useTranslations } from "next-intl"

export function Footer() {
  const t = useTranslations("footer")
  const tCommon = useTranslations("common")

  const footerSections = [
    {
      key: "product",
      links: [
        { key: "features", href: "/features" },
        { key: "pricing", href: "/pricing" },
        { key: "integrations", href: "/integrations" },
        { key: "security", href: "/security" },
        { key: "roadmap", href: "/roadmap" },
      ],
    },
    {
      key: "solutions",
      links: [
        { key: "enterprise", href: "/enterprise" },
        { key: "business", href: "/business" },
        { key: "financial", href: "/financial" },
        { key: "healthcare", href: "/healthcare" },
        { key: "government", href: "/government" },
      ],
    },
    {
      key: "resources",
      links: [
        { key: "blog", href: "/blog" },
        { key: "help", href: "/help" },
        { key: "docs", href: "/docs" },
        { key: "webinars", href: "/webinars" },
        { key: "caseStudies", href: "/case-studies" },
      ],
    },
    {
      key: "company",
      links: [
        { key: "about", href: "/about" },
        { key: "careers", href: "/careers" },
        { key: "contact", href: "/contact" },
        { key: "partners", href: "/partners" },
        { key: "news", href: "/news" },
      ],
    },
  ]

  return (
    <footer className="bg-gray-900 text-white">
      {/* Main Footer */}
      <div className="container mx-auto px-6 py-16">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-12">
          {/* Brand Column */}
          <div className="lg:col-span-2">
            <Link href="/" className="flex items-center gap-3 mb-6">
              <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center shadow-lg">
                <span className="text-white font-bold text-2xl">ش</span>
              </div>
              <div>
                <span className="text-2xl font-bold block">{tCommon("appName")}</span>
                <span className="text-sm text-gray-400">{tCommon("appNameFull")}</span>
              </div>
            </Link>
            <p className="text-gray-400 mb-6 max-w-sm">
              {t("description")}
            </p>

            {/* Contact Info */}
            <div className="space-y-3">
              <a href="mailto:info@shahin-ai.com" className="flex items-center gap-3 text-gray-400 hover:text-emerald-400 transition-colors">
                <Mail className="w-5 h-5" />
                <span>info@shahin-ai.com</span>
              </a>
              <a href="tel:+966500000000" className="flex items-center gap-3 text-gray-400 hover:text-emerald-400 transition-colors">
                <Phone className="w-5 h-5" />
                <span dir="ltr">+966 50 000 0000</span>
              </a>
              <div className="flex items-center gap-3 text-gray-400">
                <MapPin className="w-5 h-5" />
                <span>{t("location")}</span>
              </div>
            </div>

            {/* Social Links */}
            <div className="flex gap-4 mt-6">
              <a href="#" className="w-10 h-10 rounded-lg bg-gray-800 flex items-center justify-center hover:bg-emerald-600 transition-colors">
                <Twitter className="w-5 h-5" />
              </a>
              <a href="#" className="w-10 h-10 rounded-lg bg-gray-800 flex items-center justify-center hover:bg-emerald-600 transition-colors">
                <Linkedin className="w-5 h-5" />
              </a>
            </div>
          </div>

          {/* Links Columns */}
          {footerSections.map((section) => (
            <div key={section.key}>
              <h4 className="font-semibold text-white mb-4">{t(`${section.key}.title`)}</h4>
              <ul className="space-y-3">
                {section.links.map((link) => (
                  <li key={link.key}>
                    <Link
                      href={link.href}
                      className="text-gray-400 hover:text-emerald-400 transition-colors text-sm"
                    >
                      {t(`${section.key}.${link.key}`)}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-gray-800">
        <div className="container mx-auto px-6 py-6">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <div className="text-sm text-gray-400">
              {t("copyright", { appName: tCommon("appName") })}
            </div>
            <div className="flex gap-6 text-sm">
              <Link href="/privacy" className="text-gray-400 hover:text-white transition-colors">
                {t("legal.privacy")}
              </Link>
              <Link href="/terms" className="text-gray-400 hover:text-white transition-colors">
                {t("legal.terms")}
              </Link>
              <Link href="/cookies" className="text-gray-400 hover:text-white transition-colors">
                {t("legal.cookies")}
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  )
}
