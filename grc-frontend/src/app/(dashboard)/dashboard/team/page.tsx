"use client"

import { motion } from "framer-motion"
import { Users, Globe } from "lucide-react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { TeamEngagementDashboard } from "@/components/trial/team-engagement-dashboard"
import { EcosystemPartners } from "@/components/trial/ecosystem-partners"

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 }
  }
}

export default function TeamPage() {
  return (
    <motion.div
      className="space-y-6"
      variants={containerVariants}
      initial="hidden"
      animate="visible"
    >
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            Team & Ecosystem
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            Manage your team collaboration and ecosystem partnerships
          </p>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="engagement" className="w-full">
        <TabsList className="grid w-full max-w-md grid-cols-2">
          <TabsTrigger value="engagement" className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            Team Engagement
          </TabsTrigger>
          <TabsTrigger value="ecosystem" className="flex items-center gap-2">
            <Globe className="h-4 w-4" />
            Ecosystem Partners
          </TabsTrigger>
        </TabsList>

        <TabsContent value="engagement" className="mt-6">
          <TeamEngagementDashboard />
        </TabsContent>

        <TabsContent value="ecosystem" className="mt-6">
          <EcosystemPartners />
        </TabsContent>
      </Tabs>
    </motion.div>
  )
}
