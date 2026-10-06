export type Role = 'Admin' | 'Viewer'
export type NoticeStatus = 'Draft' | 'Published' | 'Archived'

export interface User {
  id: string
  username: string
  displayName: string
  role: Role
}

export interface LoginResponse {
  token: string
  expiresAtUtc: string
  user: User
}

export interface Notice {
  id: string
  title: string
  summary: string
  content: string
  category: string
  priority: number
  publishFromUtc: string
  publishUntilUtc: string | null
  status: NoticeStatus
  createdByUserId: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface SaveNoticeRequest {
  title: string
  summary: string
  content: string
  category: string
  priority: number
  publishFromUtc: string
  publishUntilUtc: string | null
  status: NoticeStatus
}

export interface StorageInfo {
  provider: string
  isFallback: boolean
  message: string
}
