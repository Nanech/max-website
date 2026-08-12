export interface SocialLink {
  label: string
  url: string
}

export interface TeamMember {
  id: string
  name: string
  role: string
  contactLinks: SocialLink[]
  subscribeLinks?: SocialLink[]
}
