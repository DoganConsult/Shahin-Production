/**
 * Hook for fetching countries from the database
 */

import { useState, useEffect } from 'react'
import { countriesApi, Country } from '../api'

export function useCountries() {
  const [countries, setCountries] = useState<Country[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function fetchCountries() {
      try {
        setIsLoading(true)
        const response = await countriesApi.getActive()
        setCountries(response.items || [])
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch countries')
        // Fallback to default countries if API fails
        setCountries(DEFAULT_COUNTRIES)
      } finally {
        setIsLoading(false)
      }
    }

    fetchCountries()
  }, [])

  return { countries, isLoading, error }
}

// Standard countries - GCC + MENA region (sorted by relevance for Saudi market)
const DEFAULT_COUNTRIES: Country[] = [
  // GCC Countries (Primary market)
  {
    id: '1',
    code: 'SA',
    name: 'Saudi Arabia',
    nameAr: 'المملكة العربية السعودية',
    iso2: 'SA',
    iso3: 'SAU',
    phoneCode: '+966',
    currency: 'SAR',
    currencyAr: 'ريال سعودي',
    region: 'GCC',
    regionAr: 'مجلس التعاون الخليجي',
    flag: '🇸🇦',
    isActive: true,
    sortOrder: 1
  },
  {
    id: '2',
    code: 'AE',
    name: 'United Arab Emirates',
    nameAr: 'الإمارات العربية المتحدة',
    iso2: 'AE',
    iso3: 'ARE',
    phoneCode: '+971',
    currency: 'AED',
    currencyAr: 'درهم إماراتي',
    region: 'GCC',
    regionAr: 'مجلس التعاون الخليجي',
    flag: '🇦🇪',
    isActive: true,
    sortOrder: 2
  },
  {
    id: '3',
    code: 'KW',
    name: 'Kuwait',
    nameAr: 'الكويت',
    iso2: 'KW',
    iso3: 'KWT',
    phoneCode: '+965',
    currency: 'KWD',
    currencyAr: 'دينار كويتي',
    region: 'GCC',
    regionAr: 'مجلس التعاون الخليجي',
    flag: '🇰🇼',
    isActive: true,
    sortOrder: 3
  },
  {
    id: '4',
    code: 'BH',
    name: 'Bahrain',
    nameAr: 'البحرين',
    iso2: 'BH',
    iso3: 'BHR',
    phoneCode: '+973',
    currency: 'BHD',
    currencyAr: 'دينار بحريني',
    region: 'GCC',
    regionAr: 'مجلس التعاون الخليجي',
    flag: '🇧🇭',
    isActive: true,
    sortOrder: 4
  },
  {
    id: '5',
    code: 'QA',
    name: 'Qatar',
    nameAr: 'قطر',
    iso2: 'QA',
    iso3: 'QAT',
    phoneCode: '+974',
    currency: 'QAR',
    currencyAr: 'ريال قطري',
    region: 'GCC',
    regionAr: 'مجلس التعاون الخليجي',
    flag: '🇶🇦',
    isActive: true,
    sortOrder: 5
  },
  {
    id: '6',
    code: 'OM',
    name: 'Oman',
    nameAr: 'عُمان',
    iso2: 'OM',
    iso3: 'OMN',
    phoneCode: '+968',
    currency: 'OMR',
    currencyAr: 'ريال عماني',
    region: 'GCC',
    regionAr: 'مجلس التعاون الخليجي',
    flag: '🇴🇲',
    isActive: true,
    sortOrder: 6
  },
  // MENA Countries
  {
    id: '7',
    code: 'EG',
    name: 'Egypt',
    nameAr: 'مصر',
    iso2: 'EG',
    iso3: 'EGY',
    phoneCode: '+20',
    currency: 'EGP',
    currencyAr: 'جنيه مصري',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇪🇬',
    isActive: true,
    sortOrder: 7
  },
  {
    id: '8',
    code: 'JO',
    name: 'Jordan',
    nameAr: 'الأردن',
    iso2: 'JO',
    iso3: 'JOR',
    phoneCode: '+962',
    currency: 'JOD',
    currencyAr: 'دينار أردني',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇯🇴',
    isActive: true,
    sortOrder: 8
  },
  {
    id: '9',
    code: 'LB',
    name: 'Lebanon',
    nameAr: 'لبنان',
    iso2: 'LB',
    iso3: 'LBN',
    phoneCode: '+961',
    currency: 'LBP',
    currencyAr: 'ليرة لبنانية',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇱🇧',
    isActive: true,
    sortOrder: 9
  },
  {
    id: '10',
    code: 'IQ',
    name: 'Iraq',
    nameAr: 'العراق',
    iso2: 'IQ',
    iso3: 'IRQ',
    phoneCode: '+964',
    currency: 'IQD',
    currencyAr: 'دينار عراقي',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇮🇶',
    isActive: true,
    sortOrder: 10
  },
  {
    id: '11',
    code: 'MA',
    name: 'Morocco',
    nameAr: 'المغرب',
    iso2: 'MA',
    iso3: 'MAR',
    phoneCode: '+212',
    currency: 'MAD',
    currencyAr: 'درهم مغربي',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇲🇦',
    isActive: true,
    sortOrder: 11
  },
  {
    id: '12',
    code: 'TN',
    name: 'Tunisia',
    nameAr: 'تونس',
    iso2: 'TN',
    iso3: 'TUN',
    phoneCode: '+216',
    currency: 'TND',
    currencyAr: 'دينار تونسي',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇹🇳',
    isActive: true,
    sortOrder: 12
  },
  {
    id: '13',
    code: 'DZ',
    name: 'Algeria',
    nameAr: 'الجزائر',
    iso2: 'DZ',
    iso3: 'DZA',
    phoneCode: '+213',
    currency: 'DZD',
    currencyAr: 'دينار جزائري',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇩🇿',
    isActive: true,
    sortOrder: 13
  },
  {
    id: '14',
    code: 'LY',
    name: 'Libya',
    nameAr: 'ليبيا',
    iso2: 'LY',
    iso3: 'LBY',
    phoneCode: '+218',
    currency: 'LYD',
    currencyAr: 'دينار ليبي',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇱🇾',
    isActive: true,
    sortOrder: 14
  },
  {
    id: '15',
    code: 'SD',
    name: 'Sudan',
    nameAr: 'السودان',
    iso2: 'SD',
    iso3: 'SDN',
    phoneCode: '+249',
    currency: 'SDG',
    currencyAr: 'جنيه سوداني',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇸🇩',
    isActive: true,
    sortOrder: 15
  },
  {
    id: '16',
    code: 'YE',
    name: 'Yemen',
    nameAr: 'اليمن',
    iso2: 'YE',
    iso3: 'YEM',
    phoneCode: '+967',
    currency: 'YER',
    currencyAr: 'ريال يمني',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇾🇪',
    isActive: true,
    sortOrder: 16
  },
  {
    id: '17',
    code: 'SY',
    name: 'Syria',
    nameAr: 'سوريا',
    iso2: 'SY',
    iso3: 'SYR',
    phoneCode: '+963',
    currency: 'SYP',
    currencyAr: 'ليرة سورية',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇸🇾',
    isActive: true,
    sortOrder: 17
  },
  {
    id: '18',
    code: 'PS',
    name: 'Palestine',
    nameAr: 'فلسطين',
    iso2: 'PS',
    iso3: 'PSE',
    phoneCode: '+970',
    currency: 'ILS',
    currencyAr: 'شيكل',
    region: 'MENA',
    regionAr: 'الشرق الأوسط وشمال أفريقيا',
    flag: '🇵🇸',
    isActive: true,
    sortOrder: 18
  },
  // International
  {
    id: '19',
    code: 'TR',
    name: 'Turkey',
    nameAr: 'تركيا',
    iso2: 'TR',
    iso3: 'TUR',
    phoneCode: '+90',
    currency: 'TRY',
    currencyAr: 'ليرة تركية',
    region: 'International',
    regionAr: 'دولي',
    flag: '🇹🇷',
    isActive: true,
    sortOrder: 19
  },
  {
    id: '20',
    code: 'PK',
    name: 'Pakistan',
    nameAr: 'باكستان',
    iso2: 'PK',
    iso3: 'PAK',
    phoneCode: '+92',
    currency: 'PKR',
    currencyAr: 'روبية باكستانية',
    region: 'International',
    regionAr: 'دولي',
    flag: '🇵🇰',
    isActive: true,
    sortOrder: 20
  },
  {
    id: '21',
    code: 'IN',
    name: 'India',
    nameAr: 'الهند',
    iso2: 'IN',
    iso3: 'IND',
    phoneCode: '+91',
    currency: 'INR',
    currencyAr: 'روبية هندية',
    region: 'International',
    regionAr: 'دولي',
    flag: '🇮🇳',
    isActive: true,
    sortOrder: 21
  },
  {
    id: '22',
    code: 'GB',
    name: 'United Kingdom',
    nameAr: 'المملكة المتحدة',
    iso2: 'GB',
    iso3: 'GBR',
    phoneCode: '+44',
    currency: 'GBP',
    currencyAr: 'جنيه إسترليني',
    region: 'International',
    regionAr: 'دولي',
    flag: '🇬🇧',
    isActive: true,
    sortOrder: 22
  },
  {
    id: '23',
    code: 'US',
    name: 'United States',
    nameAr: 'الولايات المتحدة',
    iso2: 'US',
    iso3: 'USA',
    phoneCode: '+1',
    currency: 'USD',
    currencyAr: 'دولار أمريكي',
    region: 'International',
    regionAr: 'دولي',
    flag: '🇺🇸',
    isActive: true,
    sortOrder: 23
  }
]

export default useCountries
