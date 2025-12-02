import { useQuery } from '@apollo/client'
import { GET_DIAGNOSES } from '@/lib/queries'
import { format } from 'date-fns'
import { cs } from 'date-fns/locale'

export default function DiagnosesList() {
  const { loading, error, data } = useQuery(GET_DIAGNOSES, {
    variables: {
      type: '',
      createdAfter: '2023-01-01T00:00:00Z',
      skip: 0,
      take: 10
    }
  })

  if (loading) return <div>Načítání...</div>
  if (error) return <div>Chyba: {error.message}</div>

  const diagnoses = data?.diagnoses?.nodes || []

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold mb-6">Seznam diagnóz</h2>
      
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Pacient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Diagnóza
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Poznámky
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Datum
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {diagnoses.map((diagnosis: any) => (
              <tr key={diagnosis.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {diagnosis.patientId}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {diagnosis.diagnosis}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {diagnosis.notes}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {format(new Date(diagnosis.timestampUtc), 'd. M. yyyy HH:mm', { locale: cs })}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {diagnoses.length === 0 && (
        <div className="text-center py-8 text-gray-500">
          Nebyly nalezeny žádné diagnózy
        </div>
      )}
    </div>
  )
}