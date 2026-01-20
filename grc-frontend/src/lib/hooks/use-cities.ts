/**
 * Hook for fetching cities from the database
 */

import { useState, useEffect } from 'react'
import { citiesApi, City } from '../api'

export function useCities(countryCode?: string) {
  const [cities, setCities] = useState<City[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function fetchCities() {
      try {
        setIsLoading(true)
        if (countryCode) {
          const response = await citiesApi.getActiveByCountry(countryCode)
          setCities(response.items || [])
        } else {
          setCities([])
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch cities')
        // Fallback to default cities if API fails
        if (countryCode) {
          setCities(getDefaultCitiesByCountry(countryCode))
        }
      } finally {
        setIsLoading(false)
      }
    }

    fetchCities()
  }, [countryCode])

  return { cities, isLoading, error }
}

// Get default cities by country code
function getDefaultCitiesByCountry(countryCode: string): City[] {
  return DEFAULT_CITIES.filter(city => city.countryCode === countryCode)
}

// Standard cities for GCC and MENA countries
const DEFAULT_CITIES: City[] = [
  // Saudi Arabia Cities (13 major administrative regions)
  { id: '1', code: 'RUH', name: 'Riyadh', nameAr: 'الرياض', countryCode: 'SA', countryId: '1', region: 'Riyadh Region', regionAr: 'منطقة الرياض', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '2', code: 'JED', name: 'Jeddah', nameAr: 'جدة', countryCode: 'SA', countryId: '1', region: 'Makkah Region', regionAr: 'منطقة مكة المكرمة', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '3', code: 'MKK', name: 'Makkah', nameAr: 'مكة المكرمة', countryCode: 'SA', countryId: '1', region: 'Makkah Region', regionAr: 'منطقة مكة المكرمة', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '4', code: 'MED', name: 'Madinah', nameAr: 'المدينة المنورة', countryCode: 'SA', countryId: '1', region: 'Madinah Region', regionAr: 'منطقة المدينة المنورة', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '5', code: 'DMM', name: 'Dammam', nameAr: 'الدمام', countryCode: 'SA', countryId: '1', region: 'Eastern Region', regionAr: 'المنطقة الشرقية', isCapital: false, isActive: true, sortOrder: 5 },
  { id: '6', code: 'KHO', name: 'Khobar', nameAr: 'الخبر', countryCode: 'SA', countryId: '1', region: 'Eastern Region', regionAr: 'المنطقة الشرقية', isCapital: false, isActive: true, sortOrder: 6 },
  { id: '7', code: 'DHR', name: 'Dhahran', nameAr: 'الظهران', countryCode: 'SA', countryId: '1', region: 'Eastern Region', regionAr: 'المنطقة الشرقية', isCapital: false, isActive: true, sortOrder: 7 },
  { id: '8', code: 'TAI', name: 'Taif', nameAr: 'الطائف', countryCode: 'SA', countryId: '1', region: 'Makkah Region', regionAr: 'منطقة مكة المكرمة', isCapital: false, isActive: true, sortOrder: 8 },
  { id: '9', code: 'TAB', name: 'Tabuk', nameAr: 'تبوك', countryCode: 'SA', countryId: '1', region: 'Tabuk Region', regionAr: 'منطقة تبوك', isCapital: false, isActive: true, sortOrder: 9 },
  { id: '10', code: 'BUR', name: 'Buraydah', nameAr: 'بريدة', countryCode: 'SA', countryId: '1', region: 'Qassim Region', regionAr: 'منطقة القصيم', isCapital: false, isActive: true, sortOrder: 10 },
  { id: '11', code: 'KHA', name: 'Khamis Mushait', nameAr: 'خميس مشيط', countryCode: 'SA', countryId: '1', region: 'Asir Region', regionAr: 'منطقة عسير', isCapital: false, isActive: true, sortOrder: 11 },
  { id: '12', code: 'ABH', name: 'Abha', nameAr: 'أبها', countryCode: 'SA', countryId: '1', region: 'Asir Region', regionAr: 'منطقة عسير', isCapital: false, isActive: true, sortOrder: 12 },
  { id: '13', code: 'HAI', name: 'Hail', nameAr: 'حائل', countryCode: 'SA', countryId: '1', region: 'Hail Region', regionAr: 'منطقة حائل', isCapital: false, isActive: true, sortOrder: 13 },
  { id: '14', code: 'NAJ', name: 'Najran', nameAr: 'نجران', countryCode: 'SA', countryId: '1', region: 'Najran Region', regionAr: 'منطقة نجران', isCapital: false, isActive: true, sortOrder: 14 },
  { id: '15', code: 'JIZ', name: 'Jizan', nameAr: 'جازان', countryCode: 'SA', countryId: '1', region: 'Jazan Region', regionAr: 'منطقة جازان', isCapital: false, isActive: true, sortOrder: 15 },
  { id: '16', code: 'JUB', name: 'Jubail', nameAr: 'الجبيل', countryCode: 'SA', countryId: '1', region: 'Eastern Region', regionAr: 'المنطقة الشرقية', isCapital: false, isActive: true, sortOrder: 16 },
  { id: '17', code: 'YAN', name: 'Yanbu', nameAr: 'ينبع', countryCode: 'SA', countryId: '1', region: 'Madinah Region', regionAr: 'منطقة المدينة المنورة', isCapital: false, isActive: true, sortOrder: 17 },
  { id: '18', code: 'AHC', name: 'Al-Ahsa', nameAr: 'الأحساء', countryCode: 'SA', countryId: '1', region: 'Eastern Region', regionAr: 'المنطقة الشرقية', isCapital: false, isActive: true, sortOrder: 18 },
  { id: '19', code: 'SKA', name: 'Sakaka', nameAr: 'سكاكا', countryCode: 'SA', countryId: '1', region: 'Al Jawf Region', regionAr: 'منطقة الجوف', isCapital: false, isActive: true, sortOrder: 19 },
  { id: '20', code: 'ARA', name: 'Arar', nameAr: 'عرعر', countryCode: 'SA', countryId: '1', region: 'Northern Borders', regionAr: 'منطقة الحدود الشمالية', isCapital: false, isActive: true, sortOrder: 20 },
  { id: '21', code: 'BAH', name: 'Al Bahah', nameAr: 'الباحة', countryCode: 'SA', countryId: '1', region: 'Al Bahah Region', regionAr: 'منطقة الباحة', isCapital: false, isActive: true, sortOrder: 21 },

  // UAE Cities
  { id: '22', code: 'DXB', name: 'Dubai', nameAr: 'دبي', countryCode: 'AE', countryId: '2', isCapital: false, isActive: true, sortOrder: 1 },
  { id: '23', code: 'AUH', name: 'Abu Dhabi', nameAr: 'أبوظبي', countryCode: 'AE', countryId: '2', isCapital: true, isActive: true, sortOrder: 2 },
  { id: '24', code: 'SHJ', name: 'Sharjah', nameAr: 'الشارقة', countryCode: 'AE', countryId: '2', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '25', code: 'AJM', name: 'Ajman', nameAr: 'عجمان', countryCode: 'AE', countryId: '2', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '26', code: 'RAK', name: 'Ras Al Khaimah', nameAr: 'رأس الخيمة', countryCode: 'AE', countryId: '2', isCapital: false, isActive: true, sortOrder: 5 },
  { id: '27', code: 'FUJ', name: 'Fujairah', nameAr: 'الفجيرة', countryCode: 'AE', countryId: '2', isCapital: false, isActive: true, sortOrder: 6 },
  { id: '28', code: 'UAQ', name: 'Umm Al Quwain', nameAr: 'أم القيوين', countryCode: 'AE', countryId: '2', isCapital: false, isActive: true, sortOrder: 7 },

  // Kuwait Cities
  { id: '29', code: 'KWI', name: 'Kuwait City', nameAr: 'مدينة الكويت', countryCode: 'KW', countryId: '3', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '30', code: 'HAW', name: 'Hawalli', nameAr: 'حولي', countryCode: 'KW', countryId: '3', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '31', code: 'SAL', name: 'Salmiya', nameAr: 'السالمية', countryCode: 'KW', countryId: '3', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '32', code: 'JAH', name: 'Jahra', nameAr: 'الجهراء', countryCode: 'KW', countryId: '3', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '33', code: 'AHM', name: 'Ahmadi', nameAr: 'الأحمدي', countryCode: 'KW', countryId: '3', isCapital: false, isActive: true, sortOrder: 5 },

  // Bahrain Cities
  { id: '34', code: 'MAN', name: 'Manama', nameAr: 'المنامة', countryCode: 'BH', countryId: '4', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '35', code: 'MUH', name: 'Muharraq', nameAr: 'المحرق', countryCode: 'BH', countryId: '4', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '36', code: 'RIF', name: 'Riffa', nameAr: 'الرفاع', countryCode: 'BH', countryId: '4', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '37', code: 'HAM', name: 'Hamad Town', nameAr: 'مدينة حمد', countryCode: 'BH', countryId: '4', isCapital: false, isActive: true, sortOrder: 4 },

  // Qatar Cities
  { id: '38', code: 'DOH', name: 'Doha', nameAr: 'الدوحة', countryCode: 'QA', countryId: '5', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '39', code: 'WAK', name: 'Al Wakrah', nameAr: 'الوكرة', countryCode: 'QA', countryId: '5', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '40', code: 'KHO', name: 'Al Khor', nameAr: 'الخور', countryCode: 'QA', countryId: '5', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '41', code: 'RAY', name: 'Al Rayyan', nameAr: 'الريان', countryCode: 'QA', countryId: '5', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '42', code: 'LUS', name: 'Lusail', nameAr: 'لوسيل', countryCode: 'QA', countryId: '5', isCapital: false, isActive: true, sortOrder: 5 },

  // Oman Cities
  { id: '43', code: 'MUS', name: 'Muscat', nameAr: 'مسقط', countryCode: 'OM', countryId: '6', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '44', code: 'SAL', name: 'Salalah', nameAr: 'صلالة', countryCode: 'OM', countryId: '6', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '45', code: 'SOH', name: 'Sohar', nameAr: 'صحار', countryCode: 'OM', countryId: '6', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '46', code: 'NIZ', name: 'Nizwa', nameAr: 'نزوى', countryCode: 'OM', countryId: '6', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '47', code: 'SUR', name: 'Sur', nameAr: 'صور', countryCode: 'OM', countryId: '6', isCapital: false, isActive: true, sortOrder: 5 },

  // Egypt Cities
  { id: '48', code: 'CAI', name: 'Cairo', nameAr: 'القاهرة', countryCode: 'EG', countryId: '7', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '49', code: 'ALX', name: 'Alexandria', nameAr: 'الإسكندرية', countryCode: 'EG', countryId: '7', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '50', code: 'GIZ', name: 'Giza', nameAr: 'الجيزة', countryCode: 'EG', countryId: '7', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '51', code: 'SHE', name: 'Sharm El Sheikh', nameAr: 'شرم الشيخ', countryCode: 'EG', countryId: '7', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '52', code: 'LUX', name: 'Luxor', nameAr: 'الأقصر', countryCode: 'EG', countryId: '7', isCapital: false, isActive: true, sortOrder: 5 },

  // Jordan Cities
  { id: '53', code: 'AMM', name: 'Amman', nameAr: 'عمّان', countryCode: 'JO', countryId: '8', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '54', code: 'AQA', name: 'Aqaba', nameAr: 'العقبة', countryCode: 'JO', countryId: '8', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '55', code: 'IRB', name: 'Irbid', nameAr: 'إربد', countryCode: 'JO', countryId: '8', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '56', code: 'ZAR', name: 'Zarqa', nameAr: 'الزرقاء', countryCode: 'JO', countryId: '8', isCapital: false, isActive: true, sortOrder: 4 },

  // Lebanon Cities
  { id: '57', code: 'BEY', name: 'Beirut', nameAr: 'بيروت', countryCode: 'LB', countryId: '9', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '58', code: 'TRI', name: 'Tripoli', nameAr: 'طرابلس', countryCode: 'LB', countryId: '9', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '59', code: 'SID', name: 'Sidon', nameAr: 'صيدا', countryCode: 'LB', countryId: '9', isCapital: false, isActive: true, sortOrder: 3 },

  // Iraq Cities
  { id: '60', code: 'BGW', name: 'Baghdad', nameAr: 'بغداد', countryCode: 'IQ', countryId: '10', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '61', code: 'BSR', name: 'Basra', nameAr: 'البصرة', countryCode: 'IQ', countryId: '10', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '62', code: 'EBL', name: 'Erbil', nameAr: 'أربيل', countryCode: 'IQ', countryId: '10', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '63', code: 'NJF', name: 'Najaf', nameAr: 'النجف', countryCode: 'IQ', countryId: '10', isCapital: false, isActive: true, sortOrder: 4 },

  // Morocco Cities
  { id: '64', code: 'CMN', name: 'Casablanca', nameAr: 'الدار البيضاء', countryCode: 'MA', countryId: '11', isCapital: false, isActive: true, sortOrder: 1 },
  { id: '65', code: 'RAB', name: 'Rabat', nameAr: 'الرباط', countryCode: 'MA', countryId: '11', isCapital: true, isActive: true, sortOrder: 2 },
  { id: '66', code: 'MRK', name: 'Marrakech', nameAr: 'مراكش', countryCode: 'MA', countryId: '11', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '67', code: 'FEZ', name: 'Fez', nameAr: 'فاس', countryCode: 'MA', countryId: '11', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '68', code: 'TNG', name: 'Tangier', nameAr: 'طنجة', countryCode: 'MA', countryId: '11', isCapital: false, isActive: true, sortOrder: 5 },

  // Tunisia Cities
  { id: '69', code: 'TUN', name: 'Tunis', nameAr: 'تونس', countryCode: 'TN', countryId: '12', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '70', code: 'SFA', name: 'Sfax', nameAr: 'صفاقس', countryCode: 'TN', countryId: '12', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '71', code: 'SOU', name: 'Sousse', nameAr: 'سوسة', countryCode: 'TN', countryId: '12', isCapital: false, isActive: true, sortOrder: 3 },

  // Algeria Cities
  { id: '72', code: 'ALG', name: 'Algiers', nameAr: 'الجزائر العاصمة', countryCode: 'DZ', countryId: '13', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '73', code: 'ORN', name: 'Oran', nameAr: 'وهران', countryCode: 'DZ', countryId: '13', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '74', code: 'CON', name: 'Constantine', nameAr: 'قسنطينة', countryCode: 'DZ', countryId: '13', isCapital: false, isActive: true, sortOrder: 3 },

  // Turkey Cities
  { id: '75', code: 'IST', name: 'Istanbul', nameAr: 'إسطنبول', countryCode: 'TR', countryId: '19', isCapital: false, isActive: true, sortOrder: 1 },
  { id: '76', code: 'ANK', name: 'Ankara', nameAr: 'أنقرة', countryCode: 'TR', countryId: '19', isCapital: true, isActive: true, sortOrder: 2 },
  { id: '77', code: 'IZM', name: 'Izmir', nameAr: 'إزمير', countryCode: 'TR', countryId: '19', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '78', code: 'ADA', name: 'Adana', nameAr: 'أضنة', countryCode: 'TR', countryId: '19', isCapital: false, isActive: true, sortOrder: 4 },
  { id: '79', code: 'ANT', name: 'Antalya', nameAr: 'أنطاليا', countryCode: 'TR', countryId: '19', isCapital: false, isActive: true, sortOrder: 5 },

  // UK Cities
  { id: '80', code: 'LON', name: 'London', nameAr: 'لندن', countryCode: 'GB', countryId: '22', isCapital: true, isActive: true, sortOrder: 1 },
  { id: '81', code: 'MAN', name: 'Manchester', nameAr: 'مانشستر', countryCode: 'GB', countryId: '22', isCapital: false, isActive: true, sortOrder: 2 },
  { id: '82', code: 'BIR', name: 'Birmingham', nameAr: 'برمنغهام', countryCode: 'GB', countryId: '22', isCapital: false, isActive: true, sortOrder: 3 },

  // US Cities
  { id: '83', code: 'NYC', name: 'New York', nameAr: 'نيويورك', countryCode: 'US', countryId: '23', isCapital: false, isActive: true, sortOrder: 1 },
  { id: '84', code: 'WAS', name: 'Washington D.C.', nameAr: 'واشنطن', countryCode: 'US', countryId: '23', isCapital: true, isActive: true, sortOrder: 2 },
  { id: '85', code: 'LAX', name: 'Los Angeles', nameAr: 'لوس أنجلوس', countryCode: 'US', countryId: '23', isCapital: false, isActive: true, sortOrder: 3 },
  { id: '86', code: 'HOU', name: 'Houston', nameAr: 'هيوستن', countryCode: 'US', countryId: '23', isCapital: false, isActive: true, sortOrder: 4 },
]

export default useCities
