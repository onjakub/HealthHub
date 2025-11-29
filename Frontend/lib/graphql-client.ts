import { ApolloClient, InMemoryCache, createHttpLink, from } from '@apollo/client'
import { setContext } from '@apollo/client/link/context'
import { getApiBase } from './api-base'

// Use environment-aware base for GraphQL. In dev on :3000, target backend :5023; in prod, relative.
const httpLink = createHttpLink({
  uri: `${getApiBase()}/graphql`,
})

// Auth middleware to add JWT token to requests
const authLink = setContext((_, { headers }) => {
  const token = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null

  // Only include Authorization header when we actually have a token
  const authHeader = token ? { authorization: `Bearer ${token}` } : {}

  return {
    headers: {
      ...headers,
      ...authHeader,
    }
  }
})

const client = new ApolloClient({
  link: from([authLink, httpLink]),
  cache: new InMemoryCache(),
  defaultOptions: {
    watchQuery: { fetchPolicy: 'cache-and-network' },
  },
})

export default client
