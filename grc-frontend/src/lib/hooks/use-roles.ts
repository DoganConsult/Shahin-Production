/**
 * Hook for fetching GRC roles from the database
 */

import { useState, useEffect } from 'react'
import { rolesApi, GrcRole } from '../api'

export function useRoles() {
  const [roles, setRoles] = useState<GrcRole[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function fetchRoles() {
      try {
        setIsLoading(true)
        const response = await rolesApi.getPublicRoles()
        setRoles(response.items || [])
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch roles')
        // Fallback to default roles if API fails
        setRoles(DEFAULT_ROLES)
      } finally {
        setIsLoading(false)
      }
    }

    fetchRoles()
  }, [])

  return { roles, isLoading, error }
}

// Default 15 predefined GRC roles as fallback when API is unavailable
const DEFAULT_ROLES: GrcRole[] = [
  {
    id: '1',
    code: 'ciso',
    name: 'Chief Information Security Officer (CISO)',
    nameAr: 'رئيس أمن المعلومات',
    description: 'Executive responsible for information security strategy',
    descriptionAr: 'المسؤول التنفيذي عن استراتيجية أمن المعلومات',
    permissions: [],
    sortOrder: 1
  },
  {
    id: '2',
    code: 'cro',
    name: 'Chief Risk Officer (CRO)',
    nameAr: 'رئيس إدارة المخاطر',
    description: 'Executive responsible for enterprise risk management',
    descriptionAr: 'المسؤول التنفيذي عن إدارة المخاطر المؤسسية',
    permissions: [],
    sortOrder: 2
  },
  {
    id: '3',
    code: 'cco',
    name: 'Chief Compliance Officer (CCO)',
    nameAr: 'رئيس الامتثال',
    description: 'Executive responsible for regulatory compliance',
    descriptionAr: 'المسؤول التنفيذي عن الامتثال التنظيمي',
    permissions: [],
    sortOrder: 3
  },
  {
    id: '4',
    code: 'compliance_officer',
    name: 'Compliance Officer',
    nameAr: 'مسؤول الامتثال',
    description: 'Manages compliance programs and assessments',
    descriptionAr: 'يدير برامج الامتثال والتقييمات',
    permissions: [],
    sortOrder: 4
  },
  {
    id: '5',
    code: 'risk_manager',
    name: 'Risk Manager',
    nameAr: 'مدير المخاطر',
    description: 'Identifies and manages organizational risks',
    descriptionAr: 'يحدد ويدير مخاطر المنظمة',
    permissions: [],
    sortOrder: 5
  },
  {
    id: '6',
    code: 'internal_auditor',
    name: 'Internal Auditor',
    nameAr: 'مدقق داخلي',
    description: 'Performs internal audits and assessments',
    descriptionAr: 'يقوم بالتدقيق الداخلي والتقييمات',
    permissions: [],
    sortOrder: 6
  },
  {
    id: '7',
    code: 'it_security_officer',
    name: 'IT Security Officer',
    nameAr: 'مسؤول أمن تقنية المعلومات',
    description: 'Manages IT security controls and incidents',
    descriptionAr: 'يدير ضوابط أمن تقنية المعلومات والحوادث',
    permissions: [],
    sortOrder: 7
  },
  {
    id: '8',
    code: 'dpo',
    name: 'Data Protection Officer (DPO)',
    nameAr: 'مسؤول حماية البيانات',
    description: 'Ensures data privacy and PDPL compliance',
    descriptionAr: 'يضمن خصوصية البيانات والامتثال لنظام حماية البيانات',
    permissions: [],
    sortOrder: 8
  },
  {
    id: '9',
    code: 'governance_officer',
    name: 'Governance Officer',
    nameAr: 'مسؤول الحوكمة',
    description: 'Oversees corporate governance frameworks',
    descriptionAr: 'يشرف على أطر حوكمة الشركات',
    permissions: [],
    sortOrder: 9
  },
  {
    id: '10',
    code: 'policy_manager',
    name: 'Policy Manager',
    nameAr: 'مدير السياسات',
    description: 'Develops and manages organizational policies',
    descriptionAr: 'يطور ويدير سياسات المنظمة',
    permissions: [],
    sortOrder: 10
  },
  {
    id: '11',
    code: 'internal_control_manager',
    name: 'Internal Control Manager',
    nameAr: 'مدير الرقابة الداخلية',
    description: 'Manages internal control systems',
    descriptionAr: 'يدير أنظمة الرقابة الداخلية',
    permissions: [],
    sortOrder: 11
  },
  {
    id: '12',
    code: 'bcm_manager',
    name: 'Business Continuity Manager',
    nameAr: 'مدير استمرارية الأعمال',
    description: 'Manages business continuity and disaster recovery',
    descriptionAr: 'يدير استمرارية الأعمال والتعافي من الكوارث',
    permissions: [],
    sortOrder: 12
  },
  {
    id: '13',
    code: 'legal_counsel',
    name: 'Legal Counsel',
    nameAr: 'المستشار القانوني',
    description: 'Provides legal guidance on compliance matters',
    descriptionAr: 'يقدم التوجيه القانوني في شؤون الامتثال',
    permissions: [],
    sortOrder: 13
  },
  {
    id: '14',
    code: 'business_owner',
    name: 'Business Process Owner',
    nameAr: 'مالك العمليات التجارية',
    description: 'Owns and manages business processes',
    descriptionAr: 'يمتلك ويدير العمليات التجارية',
    permissions: [],
    sortOrder: 14
  },
  {
    id: '15',
    code: 'viewer',
    name: 'Viewer / Stakeholder',
    nameAr: 'مشاهد / صاحب مصلحة',
    description: 'Read-only access to reports and dashboards',
    descriptionAr: 'صلاحية القراءة فقط للتقارير ولوحات المعلومات',
    permissions: [],
    sortOrder: 15
  }
]

export default useRoles
