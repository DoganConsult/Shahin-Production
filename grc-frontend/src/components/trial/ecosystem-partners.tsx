"use client"

import { useState } from "react"
import { motion } from "framer-motion"
import {
  Building2,
  Shield,
  Users,
  Star,
  Link2,
  Search,
  CheckCircle,
  Clock,
  ExternalLink,
} from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import {
  useEcosystemPartners,
  useEcosystemConnections,
  useRequestPartnerConnection,
} from "@/lib/hooks/use-trial"
import type { EcosystemPartner } from "@/lib/api/trial"

const partnerTypeIcons = {
  consultant: Building2,
  auditor: Shield,
  vendor: Users,
}

const partnerTypeColors = {
  consultant: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
  auditor: "bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300",
  vendor: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300",
}

export function EcosystemPartners() {
  const [selectedSector, setSelectedSector] = useState<string>("")
  const [connectingId, setConnectingId] = useState<string | null>(null)

  const { data: partnersData, isLoading: loadingPartners } = useEcosystemPartners(selectedSector)
  const { data: connectionsData } = useEcosystemConnections()
  const requestConnection = useRequestPartnerConnection()

  const partners = partnersData?.partners || []
  const connections = connectionsData?.connections || []
  const connectedPartnerIds = new Set(connections.map((c) => c.partnerId))

  const handleConnect = async (partner: EcosystemPartner) => {
    setConnectingId(partner.partnerId)
    try {
      await requestConnection.mutateAsync({
        partnerId: partner.partnerId,
        partnerType: partner.type,
        partnerName: partner.name,
        connectionPurpose: `Connect with ${partner.name} for GRC collaboration`,
        sharedDataTypes: ["compliance_status", "assessment_reports"],
      })
    } finally {
      setConnectingId(null)
    }
  }

  const getConnectionStatus = (partnerId: string) => {
    const connection = connections.find((c) => c.partnerId === partnerId)
    return connection?.status
  }

  if (loadingPartners) {
    return (
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {[...Array(6)].map((_, i) => (
          <Card key={i} className="animate-pulse">
            <CardContent className="p-6">
              <div className="h-40 bg-gray-200 dark:bg-gray-700 rounded" />
            </CardContent>
          </Card>
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Ecosystem Partners</h2>
          <p className="text-muted-foreground">
            Connect with trusted GRC partners in your region
          </p>
        </div>

        {/* Sector Filter */}
        <div className="flex items-center gap-2">
          <Button
            variant={selectedSector === "" ? "default" : "outline"}
            size="sm"
            onClick={() => setSelectedSector("")}
          >
            All
          </Button>
          <Button
            variant={selectedSector === "finance" ? "default" : "outline"}
            size="sm"
            onClick={() => setSelectedSector("finance")}
          >
            Finance
          </Button>
          <Button
            variant={selectedSector === "healthcare" ? "default" : "outline"}
            size="sm"
            onClick={() => setSelectedSector("healthcare")}
          >
            Healthcare
          </Button>
          <Button
            variant={selectedSector === "technology" ? "default" : "outline"}
            size="sm"
            onClick={() => setSelectedSector("technology")}
          >
            Technology
          </Button>
        </div>
      </div>

      {/* Partners Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {partners.map((partner, index) => {
          const Icon = partnerTypeIcons[partner.type as keyof typeof partnerTypeIcons] || Building2
          const colorClass =
            partnerTypeColors[partner.type as keyof typeof partnerTypeColors] ||
            partnerTypeColors.consultant
          const status = getConnectionStatus(partner.partnerId)
          const isConnected = connectedPartnerIds.has(partner.partnerId)

          return (
            <motion.div
              key={partner.partnerId}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <Card className="h-full flex flex-col">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                      <div
                        className={`h-12 w-12 rounded-xl flex items-center justify-center ${colorClass}`}
                      >
                        <Icon className="h-6 w-6" />
                      </div>
                      <div>
                        <CardTitle className="text-lg">{partner.name}</CardTitle>
                        <Badge variant="secondary" className="mt-1 capitalize">
                          {partner.type}
                        </Badge>
                      </div>
                    </div>
                    <div className="flex items-center gap-1 text-amber-500">
                      <Star className="h-4 w-4 fill-current" />
                      <span className="font-medium">{partner.rating}</span>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="flex-1 flex flex-col">
                  <p className="text-sm text-muted-foreground mb-4 flex-1">
                    {partner.description}
                  </p>

                  {/* Services */}
                  <div className="mb-4">
                    <p className="text-xs font-medium text-muted-foreground mb-2">Services</p>
                    <div className="flex flex-wrap gap-1">
                      {partner.services.slice(0, 3).map((service) => (
                        <Badge key={service} variant="outline" className="text-xs">
                          {service}
                        </Badge>
                      ))}
                      {partner.services.length > 3 && (
                        <Badge variant="outline" className="text-xs">
                          +{partner.services.length - 3}
                        </Badge>
                      )}
                    </div>
                  </div>

                  {/* Certifications */}
                  <div className="mb-4">
                    <p className="text-xs font-medium text-muted-foreground mb-2">Certifications</p>
                    <div className="flex flex-wrap gap-1">
                      {partner.certifications.slice(0, 2).map((cert) => (
                        <Badge
                          key={cert}
                          variant="secondary"
                          className="text-xs bg-emerald-100 text-emerald-700 dark:bg-emerald-900 dark:text-emerald-300"
                        >
                          {cert}
                        </Badge>
                      ))}
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="flex items-center gap-4 text-sm text-muted-foreground mb-4">
                    <div className="flex items-center gap-1">
                      <Link2 className="h-4 w-4" />
                      <span>{partner.connections} connections</span>
                    </div>
                  </div>

                  {/* Action Button */}
                  {isConnected ? (
                    <Button variant="outline" className="w-full" disabled>
                      {status === "pending" ? (
                        <>
                          <Clock className="h-4 w-4 mr-2" />
                          Pending
                        </>
                      ) : status === "approved" ? (
                        <>
                          <CheckCircle className="h-4 w-4 mr-2" />
                          Connected
                        </>
                      ) : (
                        <>
                          <Link2 className="h-4 w-4 mr-2" />
                          {status}
                        </>
                      )}
                    </Button>
                  ) : (
                    <Button
                      variant="default"
                      className="w-full"
                      onClick={() => handleConnect(partner)}
                      disabled={connectingId === partner.partnerId}
                    >
                      {connectingId === partner.partnerId ? (
                        <>
                          <Clock className="h-4 w-4 mr-2 animate-spin" />
                          Connecting...
                        </>
                      ) : (
                        <>
                          <Link2 className="h-4 w-4 mr-2" />
                          Request Connection
                        </>
                      )}
                    </Button>
                  )}
                </CardContent>
              </Card>
            </motion.div>
          )
        })}
      </div>

      {partners.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <Search className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
            <h3 className="text-lg font-medium mb-2">No partners found</h3>
            <p className="text-muted-foreground">
              Try selecting a different sector or check back later
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

export default EcosystemPartners
