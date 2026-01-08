"use client"

import Link from "next/link"
import { motion } from "framer-motion"
import { Mail, Phone, MapPin, Linkedin, Twitter } from "lucide-react"

const footerLinks = {
  product: {
    title: "المنتج",
    links: [
      { name: "المميزات", href: "/features" },
      { name: "الأسعار", href: "/pricing" },
      { name: "التكاملات", href: "/integrations" },
      { name: "الأمان", href: "/security" },
      { name: "خارطة الطريق", href: "/roadmap" },
    ],
  },
  solutions: {
    title: "الحلول",
    links: [
      { name: "للشركات الكبيرة", href: "/enterprise" },
      { name: "للشركات المتوسطة", href: "/business" },
      { name: "للقطاع المالي", href: "/financial" },
      { name: "للقطاع الصحي", href: "/healthcare" },
      { name: "للقطاع الحكومي", href: "/government" },
    ],
  },
  resources: {
    title: "الموارد",
    links: [
      { name: "المدونة", href: "/blog" },
      { name: "مركز المساعدة", href: "/help" },
      { name: "الوثائق", href: "/docs" },
      { name: "الندوات", href: "/webinars" },
      { name: "دراسات الحالة", href: "/case-studies" },
    ],
  },
  company: {
    title: "الشركة",
    links: [
      { name: "عن شاهين", href: "/about" },
      { name: "الوظائف", href: "/careers" },
      { name: "تواصل معنا", href: "/contact" },
      { name: "الشركاء", href: "/partners" },
      { name: "الأخبار", href: "/news" },
    ],
  },
}

export function Footer() {
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
                <span className="text-2xl font-bold block">شاهين</span>
                <span className="text-sm text-gray-400">Shahin GRC</span>
              </div>
            </Link>
            <p className="text-gray-400 mb-6 max-w-sm">
              منصة متكاملة لإدارة الحوكمة والمخاطر والامتثال. مصممة خصيصاً للمؤسسات السعودية.
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
                <span>الرياض، المملكة العربية السعودية</span>
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
          {Object.entries(footerLinks).map(([key, section]) => (
            <div key={key}>
              <h4 className="font-semibold text-white mb-4">{section.title}</h4>
              <ul className="space-y-3">
                {section.links.map((link) => (
                  <li key={link.name}>
                    <Link 
                      href={link.href}
                      className="text-gray-400 hover:text-emerald-400 transition-colors text-sm"
                    >
                      {link.name}
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
              © 2024 شاهين. جميع الحقوق محفوظة.
            </div>
            <div className="flex gap-6 text-sm">
              <Link href="/privacy" className="text-gray-400 hover:text-white transition-colors">
                سياسة الخصوصية
              </Link>
              <Link href="/terms" className="text-gray-400 hover:text-white transition-colors">
                الشروط والأحكام
              </Link>
              <Link href="/cookies" className="text-gray-400 hover:text-white transition-colors">
                سياسة ملفات تعريف الارتباط
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  )
}
