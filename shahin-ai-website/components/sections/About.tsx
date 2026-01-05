'use client';

export default function About() {
  const team = [
    {
      name: 'فريق الخبراء',
      role: 'خبراء الامتثال والحوكمة',
      description: 'فريق من المتخصصين في الامتثال التنظيمي والأمن السيبراني',
    },
  ];

  const values = [
    {
      icon: '🎯',
      title: 'الدقة',
      description: 'نلتزم بأعلى معايير الدقة في تحليل المتطلبات التنظيمية',
    },
    {
      icon: '🤝',
      title: 'الشراكة',
      description: 'نعمل كشركاء حقيقيين مع عملائنا لتحقيق أهدافهم',
    },
    {
      icon: '💡',
      title: 'الابتكار',
      description: 'نستخدم أحدث تقنيات الذكاء الاصطناعي لتبسيط الامتثال',
    },
    {
      icon: '🔒',
      title: 'الأمان',
      description: 'نحمي بيانات عملائنا بأعلى معايير الأمان',
    },
  ];

  return (
    <section id="about" className="py-20 bg-gradient-to-b from-white to-gray-50">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          {/* Content */}
          <div>
            <h2 className="text-3xl md:text-4xl font-bold text-[#0B1F3B] mb-6">
              من نحن
            </h2>
            <p className="text-lg text-gray-600 mb-6">
              شاهين AI هي منصة سعودية رائدة في مجال الحوكمة والمخاطر والامتثال (GRC)،
              مصممة خصيصاً لتلبية متطلبات السوق السعودي والخليجي.
            </p>
            <p className="text-gray-600 mb-6">
              نستخدم تقنيات الذكاء الاصطناعي المتقدمة لمساعدة المؤسسات على تحقيق
              الامتثال بكفاءة عالية وتكلفة أقل، مع دعم كامل للأطر التنظيمية السعودية
              مثل NCA ECC و SAMA CSF و PDPL.
            </p>
            <div className="flex items-center space-x-4 rtl:space-x-reverse">
              <a
                href="#contact"
                className="bg-[#0E7490] text-white px-6 py-3 rounded-lg hover:bg-[#0A5D73] transition-colors"
              >
                تواصل معنا
              </a>
              <a
                href="#"
                className="text-[#0E7490] font-medium hover:text-[#0A5D73] transition-colors"
              >
                اعرف المزيد ←
              </a>
            </div>
          </div>

          {/* Values Grid */}
          <div className="grid grid-cols-2 gap-4">
            {values.map((value, index) => (
              <div
                key={index}
                className="bg-white rounded-xl p-6 shadow-md hover:shadow-lg transition-shadow"
              >
                <span className="text-3xl mb-3 block">{value.icon}</span>
                <h3 className="text-lg font-bold text-[#0B1F3B] mb-2">{value.title}</h3>
                <p className="text-gray-600 text-sm">{value.description}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Stats */}
        <div className="mt-16 grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
          <div>
            <div className="text-4xl font-bold text-[#0E7490]">100+</div>
            <div className="text-gray-600 mt-2">عميل نشط</div>
          </div>
          <div>
            <div className="text-4xl font-bold text-[#0E7490]">15+</div>
            <div className="text-gray-600 mt-2">إطار تنظيمي</div>
          </div>
          <div>
            <div className="text-4xl font-bold text-[#0E7490]">99%</div>
            <div className="text-gray-600 mt-2">رضا العملاء</div>
          </div>
          <div>
            <div className="text-4xl font-bold text-[#0E7490]">24/7</div>
            <div className="text-gray-600 mt-2">دعم فني</div>
          </div>
        </div>
      </div>
    </section>
  );
}
