import api from './api'

export async function getUserConnectionsByIpPrefix(prefix: string, skip = 0, take = 100) {
  const response = await api.get('api/connection-events/users/by-ip-prefix', { params: { prefix, skip, take } })
  return response.data
}

export async function getUserIps(userId: number) {
  const response = await api.get(`api/connection-events/users/${userId}/ips`)
  return response.data
}

export async function getUserLastConnection(userId: number) {
  const response = await api.get(`api/connection-events/users/${userId}/latest`)
  return response.data
}

export async function getLatestConnectionByUserAndIp(userId: number, ip: string) {
  const response = await api.get(`api/connection-events/users/${userId}/latest-by-ip`, { params: { ip } })
  return response.data
}
