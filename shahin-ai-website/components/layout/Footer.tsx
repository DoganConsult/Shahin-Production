'use client';

import Link from 'next/link';
import { Mail, Phone, MapPin, Linkedin, Twitter, Youtube } from 'lucide-react';

export default function Footer() {
  return (
    <footer className="bg-slate-900 text-white">
      <div className="container mx-auto px-6 py-16">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          {/* Brand Column */}
          <div className="col-span-1 md:col-span-2">
            <Link href="/" className="flex items-center space-x-3 mb-6">
              <div className="w-12 h-12 bg-gradient-to-br from-emerald-500 to-teal-500 rounded-xl flex items-center justify-center">
                <span className="text-white font-bold text-2xl">ش</span>
              </div>
              <div>
                <span className="text-xl font-bold text-white block">Shahin AI</span>
                <span className="text-sm text-slate-400">شاهين</span>
              </div>
            </Link>
            <p className="text-slate-400 mb-4 max-w-xs">
              One control map for all markets. Assurance automation for the modern enterprise.
            </p>
            <p className="text-slate-500 text-sm mb-6" dir="rtl">
              خريطة تحكم واحدة لكل الأسواق
            </p>
            
            <div className="space-y-3 text-sm text-slate-400">
              <a href="mailto:info@shahin-ai.com" className="flex items-center gap-2 hover:text-emerald-400 transition-colors">
                <Mail className="w-4 h-4" />
                info@shahin-ai.com
              </a>
              <div className="flex items-center gap-2">
                <MapPin className="w-4 h-4" />
                Riyadh, Saudi Arabia
              </div>
            </div>
          </div>

          {/* Product Links */}
          <div>
            <h4 className="font-semibold text-white mb-4">Product</h4>
            <ul className="space-y-3 text-sm text-slate-400">
              <li><Link href="#features" className="hover:text-emerald-400 transition-colors">Features</Link></li>
              <li><Link href="#pricing" className="hover:text-emerald-400 transition-colors">Pricing</Link></li>
              <li><Link href="#industries" className="hover:text-emerald-400 transition-colors">Industries</Link></li>
              <li><Link href="#integrations" className="hover:text-emerald-400 transition-colors">Integrations</Link></li>
            </ul>
          </div>

          {/* Company Links */}
          <div>
            <h4 className="font-semibold text-white mb-4">Company</h4>
            <ul className="space-y-3 text-sm text-slate-400">
              <li><Link href="#about" className="hover:text-emerald-400 transition-colors">About Us</Link></li>
              <li><Link href="#contact" className="hover:text-emerald-400 transition-colors">Contact</Link></li>
              <li><Link href="#privacy" className="hover:text-emerald-400 transition-colors">Privacy Policy</Link></li>
              <li><Link href="#terms" className="hover:text-emerald-400 transition-colors">Terms of Service</Link></li>
            </ul>
          </div>
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-slate-800">
        <div className="container mx-auto px-6 py-6">
          <div className="flex flex-col md:flex-row items-center justify-between gap-4">
            <div className="flex items-center gap-4 text-sm text-slate-500">
              <span>© {new Date().getFullYear()} Shahin AI. All rights reserved.</span>
              <span dir="rtl">جميع الحقوق محفوظة</span>
            </div>

            <div className="flex items-center gap-4">
              <a href="#" className="text-slate-400 hover:text-emerald-400 transition-colors">
                <Linkedin className="w-5 h-5" />
              </a>
              <a href="#" className="text-slate-400 hover:text-emerald-400 transition-colors">
                <Twitter className="w-5 h-5" />
              </a>
              <a href="#" className="text-slate-400 hover:text-emerald-400 transition-colors">
                <Youtube className="w-5 h-5" />
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
