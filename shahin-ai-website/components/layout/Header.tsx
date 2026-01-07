'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';

export default function Header() {
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        isScrolled
          ? 'bg-white/95 backdrop-blur-md shadow-lg py-3'
          : 'bg-slate-900/50 backdrop-blur-sm py-5'
      }`}
    >
      <nav className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between">
          {/* Logo */}
          <Link href="/" className="flex items-center space-x-3 rtl:space-x-reverse">
            <div className="w-10 h-10 bg-gradient-to-br from-[#0E7490] to-[#14B8A6] rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-xl">ش</span>
            </div>
            <span className="text-xl font-bold text-[#0B1F3B]">Shahin AI</span>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center space-x-6 rtl:space-x-reverse">
            <Link href="#features" className={`transition-colors ${isScrolled ? 'text-gray-700 hover:text-emerald-600' : 'text-white/80 hover:text-white'}`}>
              Features
            </Link>
            <Link href="#solutions" className={`transition-colors ${isScrolled ? 'text-gray-700 hover:text-emerald-600' : 'text-white/80 hover:text-white'}`}>
              Solutions
            </Link>
            <Link href="#industries" className={`transition-colors ${isScrolled ? 'text-gray-700 hover:text-emerald-600' : 'text-white/80 hover:text-white'}`}>
              Industries
            </Link>
            <Link href="#pricing" className={`transition-colors ${isScrolled ? 'text-gray-700 hover:text-emerald-600' : 'text-white/80 hover:text-white'}`}>
              Pricing
            </Link>
            <Link href="#about" className={`transition-colors ${isScrolled ? 'text-gray-700 hover:text-emerald-600' : 'text-white/80 hover:text-white'}`}>
              About
            </Link>
            {/* Login Icon/Button - Links to Portal Login */}
            <Link
              href="https://portal.shahin-ai.com/Account/Login"
              className="flex items-center space-x-2 rtl:space-x-reverse text-[#0E7490] hover:text-[#0A5D73] transition-colors"
              title="تسجيل الدخول"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              <span className="font-medium">تسجيل الدخول</span>
            </Link>
            <Link
              href="https://app.shahin-ai.com"
              className="bg-[#0B1F3B] text-white px-6 py-2 rounded-lg hover:bg-[#1A3A5C] transition-colors"
            >
              ابدأ مجاناً
            </Link>
          </div>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden p-2"
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            aria-label="Menu"
          >
            <svg
              className="w-6 h-6 text-gray-700"
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              {isMobileMenuOpen ? (
                <path d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>

        {/* Mobile Menu */}
        {isMobileMenuOpen && (
          <div className="md:hidden mt-4 pb-4 border-t border-gray-200">
            <div className="flex flex-col space-y-4 pt-4">
              <Link href="#features" className="text-gray-700 hover:text-[#0E7490]">
                المميزات
              </Link>
              <Link href="#solutions" className="text-gray-700 hover:text-[#0E7490]">
                الحلول
              </Link>
              <Link href="#industries" className="text-gray-700 hover:text-[#0E7490]">
                القطاعات
              </Link>
              <Link href="#pricing" className="text-gray-700 hover:text-[#0E7490]">
                الأسعار
              </Link>
              <Link href="#resources" className="text-gray-700 hover:text-[#0E7490]">
                الموارد
              </Link>
              <Link href="#about" className="text-gray-700 hover:text-[#0E7490]">
                من نحن
              </Link>
              <Link href="#contact" className="text-gray-700 hover:text-[#0E7490]">
                اتصل بنا
              </Link>
              {/* Mobile Login Button */}
              <Link
                href="https://portal.shahin-ai.com/Account/Login"
                className="flex items-center space-x-2 rtl:space-x-reverse text-[#0E7490] hover:text-[#0A5D73] font-medium"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span>تسجيل الدخول</span>
              </Link>
              <Link
                href="https://app.shahin-ai.com"
                className="bg-[#0B1F3B] text-white px-6 py-2 rounded-lg text-center"
              >
                ابدأ مجاناً
              </Link>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
}
