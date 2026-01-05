'use client';

export default function Resources() {
  const resources = [
    {
      icon: '📚',
      title: 'دليل الامتثال',
      titleEn: 'Compliance Guide',
      description: 'دليل شامل لفهم متطلبات الامتثال في المملكة العربية السعودية',
      link: '#',
      type: 'PDF',
    },
    {
      icon: '🎥',
      title: 'فيديوهات تعليمية',
      titleEn: 'Video Tutorials',
      description: 'سلسلة فيديوهات لشرح استخدام المنصة خطوة بخطوة',
      link: '#',
      type: 'Video',
    },
    {
      icon: '📋',
      title: 'قوالب السياسات',
      titleEn: 'Policy Templates',
      description: 'قوالب جاهزة للسياسات والإجراءات متوافقة مع الأطر التنظيمية',
      link: '#',
      type: 'Templates',
    },
    {
      icon: '📊',
      title: 'تقارير الصناعة',
      titleEn: 'Industry Reports',
      description: 'تقارير وتحليلات عن واقع الامتثال في مختلف القطاعات',
      link: '#',
      type: 'Report',
    },
    {
      icon: '🎓',
      title: 'ندوات وورش عمل',
      titleEn: 'Webinars',
      description: 'ندوات مباشرة مع خبراء الامتثال والأمن السيبراني',
      link: '#',
      type: 'Webinar',
    },
    {
      icon: '📖',
      title: 'مدونة شاهين',
      titleEn: 'Shahin Blog',
      description: 'مقالات ونصائح حول أفضل ممارسات الحوكمة والامتثال',
      link: '#',
      type: 'Blog',
    },
  ];

  return (
    <section id="resources" className="py-20 bg-white">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-bold text-[#0B1F3B] mb-4">
            الموارد والمعرفة
          </h2>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            موارد مجانية لمساعدتك في رحلة الامتثال
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {resources.map((resource, index) => (
            <a
              key={index}
              href={resource.link}
              className="group bg-gray-50 rounded-xl p-6 hover:bg-[#0E7490]/5 transition-all duration-300 border border-gray-100 hover:border-[#0E7490]/30"
            >
              <div className="flex items-start justify-between mb-4">
                <span className="text-3xl">{resource.icon}</span>
                <span className="px-3 py-1 bg-[#0E7490]/10 text-[#0E7490] text-xs font-medium rounded-full">
                  {resource.type}
                </span>
              </div>
              <h3 className="text-lg font-bold text-[#0B1F3B] mb-1 group-hover:text-[#0E7490] transition-colors">
                {resource.title}
              </h3>
              <p className="text-sm text-[#0E7490] mb-2">{resource.titleEn}</p>
              <p className="text-gray-600 text-sm">{resource.description}</p>
            </a>
          ))}
        </div>

        <div className="text-center mt-12">
          <a
            href="#"
            className="inline-flex items-center space-x-2 rtl:space-x-reverse text-[#0E7490] font-medium hover:text-[#0A5D73] transition-colors"
          >
            <span>عرض جميع الموارد</span>
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
            </svg>
          </a>
        </div>
      </div>
    </section>
  );
}
