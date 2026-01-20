/**
 * Hook for fetching organization sectors from the database
 */

import { useState, useEffect } from 'react'
import { sectorsApi, Sector } from '../api'

export function useSectors() {
  const [sectors, setSectors] = useState<Sector[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function fetchSectors() {
      try {
        setIsLoading(true)
        const response = await sectorsApi.getActive()
        setSectors(response.items || [])
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch sectors')
        // Fallback to default sectors if API fails
        setSectors(DEFAULT_SECTORS)
      } finally {
        setIsLoading(false)
      }
    }

    fetchSectors()
  }, [])

  return { sectors, isLoading, error }
}

// Default 14 standard sectors as fallback when API is unavailable
const DEFAULT_SECTORS: Sector[] = [
  {
    id: '1',
    code: 'financial',
    name: 'Financial Services',
    nameAr: 'الخدمات المالية',
    description: 'Banks, Insurance, Investment',
    descriptionAr: 'البنوك، التأمين، الاستثمار',
    icon: '🏦',
    isActive: true,
    sortOrder: 1
  },
  {
    id: '2',
    code: 'healthcare',
    name: 'Healthcare',
    nameAr: 'الرعاية الصحية',
    description: 'Hospitals, Clinics, Pharmaceuticals',
    descriptionAr: 'المستشفيات، العيادات، الأدوية',
    icon: '🏥',
    isActive: true,
    sortOrder: 2
  },
  {
    id: '3',
    code: 'government',
    name: 'Government & Public Sector',
    nameAr: 'القطاع الحكومي والعام',
    description: 'Ministries, Agencies, Public Services',
    descriptionAr: 'الوزارات، الهيئات، الخدمات العامة',
    icon: '🏛️',
    isActive: true,
    sortOrder: 3
  },
  {
    id: '4',
    code: 'energy',
    name: 'Energy & Utilities',
    nameAr: 'الطاقة والمرافق',
    description: 'Oil & Gas, Electricity, Water',
    descriptionAr: 'النفط والغاز، الكهرباء، المياه',
    icon: '⚡',
    isActive: true,
    sortOrder: 4
  },
  {
    id: '5',
    code: 'technology',
    name: 'Technology & IT',
    nameAr: 'التكنولوجيا وتقنية المعلومات',
    description: 'Software, IT Services, Cybersecurity',
    descriptionAr: 'البرمجيات، خدمات تقنية المعلومات، الأمن السيبراني',
    icon: '💻',
    isActive: true,
    sortOrder: 5
  },
  {
    id: '6',
    code: 'telecom',
    name: 'Telecommunications',
    nameAr: 'الاتصالات',
    description: 'Telecom, ISP, Digital Services',
    descriptionAr: 'الاتصالات، مزودي الإنترنت، الخدمات الرقمية',
    icon: '📡',
    isActive: true,
    sortOrder: 6
  },
  {
    id: '7',
    code: 'manufacturing',
    name: 'Manufacturing & Industrial',
    nameAr: 'التصنيع والصناعة',
    description: 'Manufacturing, Production, Processing',
    descriptionAr: 'التصنيع، الإنتاج، المعالجة',
    icon: '🏭',
    isActive: true,
    sortOrder: 7
  },
  {
    id: '8',
    code: 'retail',
    name: 'Retail & E-commerce',
    nameAr: 'التجزئة والتجارة الإلكترونية',
    description: 'Retail, E-commerce, Distribution',
    descriptionAr: 'التجزئة، التجارة الإلكترونية، التوزيع',
    icon: '🛒',
    isActive: true,
    sortOrder: 8
  },
  {
    id: '9',
    code: 'education',
    name: 'Education & Training',
    nameAr: 'التعليم والتدريب',
    description: 'Universities, Schools, Training Centers',
    descriptionAr: 'الجامعات، المدارس، مراكز التدريب',
    icon: '🎓',
    isActive: true,
    sortOrder: 9
  },
  {
    id: '10',
    code: 'real_estate',
    name: 'Real Estate & Construction',
    nameAr: 'العقارات والإنشاءات',
    description: 'Development, Construction, Property Management',
    descriptionAr: 'التطوير، البناء، إدارة الممتلكات',
    icon: '🏗️',
    isActive: true,
    sortOrder: 10
  },
  {
    id: '11',
    code: 'logistics',
    name: 'Logistics & Transportation',
    nameAr: 'اللوجستيات والنقل',
    description: 'Transportation, Warehousing, Supply Chain',
    descriptionAr: 'النقل، التخزين، سلسلة الإمداد',
    icon: '🚚',
    isActive: true,
    sortOrder: 11
  },
  {
    id: '12',
    code: 'tourism',
    name: 'Tourism & Hospitality',
    nameAr: 'السياحة والضيافة',
    description: 'Hotels, Tourism, Entertainment',
    descriptionAr: 'الفنادق، السياحة، الترفيه',
    icon: '🏨',
    isActive: true,
    sortOrder: 12
  },
  {
    id: '13',
    code: 'professional',
    name: 'Professional Services',
    nameAr: 'الخدمات المهنية',
    description: 'Consulting, Legal, Accounting',
    descriptionAr: 'الاستشارات، القانون، المحاسبة',
    icon: '💼',
    isActive: true,
    sortOrder: 13
  },
  {
    id: '14',
    code: 'other',
    name: 'Other',
    nameAr: 'أخرى',
    description: 'Other sectors',
    descriptionAr: 'قطاعات أخرى',
    icon: '📋',
    isActive: true,
    sortOrder: 99
  }
]

export default useSectors
