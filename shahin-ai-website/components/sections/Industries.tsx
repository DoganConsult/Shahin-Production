'use client';

export default function Industries() {
  const industries = [
    {
      icon: '🏦',
      title: 'القطاع المالي',
      titleEn: 'Financial Services',
      description: 'بنوك، شركات تأمين، شركات الوساطة المالية',
      frameworks: ['SAMA CSF', 'NCA ECC', 'PDPL'],
    },
    {
      icon: '🏥',
      title: 'الرعاية الصحية',
      titleEn: 'Healthcare',
      description: 'مستشفيات، عيادات، شركات الأدوية',
      frameworks: ['NCA ECC', 'PDPL', 'HIPAA'],
    },
    {
      icon: '🏛️',
      title: 'القطاع الحكومي',
      titleEn: 'Government',
      description: 'الوزارات، الهيئات الحكومية، المؤسسات العامة',
      frameworks: ['NCA ECC', 'NCA CSCC', 'DGA'],
    },
    {
      icon: '⚡',
      title: 'الطاقة والمرافق',
      titleEn: 'Energy & Utilities',
      description: 'شركات النفط والغاز، الكهرباء، المياه',
      frameworks: ['NCA CSCC', 'NCA ECC', 'HCIS'],
    },
    {
      icon: '🛒',
      title: 'التجزئة والتجارة',
      titleEn: 'Retail & Commerce',
      description: 'متاجر التجزئة، التجارة الإلكترونية',
      frameworks: ['PDPL', 'NCA ECC', 'PCI-DSS'],
    },
    {
      icon: '📡',
      title: 'الاتصالات والتقنية',
      titleEn: 'Telecom & Tech',
      description: 'شركات الاتصالات، مزودي الخدمات السحابية',
      frameworks: ['CST CRF', 'NCA ECC', 'PDPL'],
    },
  ];

  return (
    <section id="industries" className="py-20 bg-gradient-to-b from-gray-50 to-white">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-bold text-[#0B1F3B] mb-4">
            القطاعات التي نخدمها
          </h2>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            حلول مخصصة لكل قطاع مع دعم كامل للأطر التنظيمية المطلوبة
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {industries.map((industry, index) => (
            <div
              key={index}
              className="bg-white rounded-xl p-6 shadow-lg hover:shadow-xl transition-all duration-300 border border-gray-100 hover:border-[#0E7490]/30"
            >
              <div className="text-4xl mb-4">{industry.icon}</div>
              <h3 className="text-xl font-bold text-[#0B1F3B] mb-1">{industry.title}</h3>
              <p className="text-sm text-[#0E7490] mb-3">{industry.titleEn}</p>
              <p className="text-gray-600 mb-4">{industry.description}</p>
              <div className="flex flex-wrap gap-2">
                {industry.frameworks.map((framework, i) => (
                  <span
                    key={i}
                    className="px-3 py-1 bg-[#0E7490]/10 text-[#0E7490] text-sm rounded-full"
                  >
                    {framework}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
