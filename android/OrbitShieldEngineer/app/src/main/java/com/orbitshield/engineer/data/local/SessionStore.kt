package com.orbitshield.engineer.data.local

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.orbitshield.engineer.domain.model.AuthSession
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

private val Context.sessionDataStore by preferencesDataStore(name = "orbit_shield_session")

class SessionStore(private val context: Context) {
    val session: Flow<AuthSession?> = context.sessionDataStore.data.map { preferences ->
        val baseUrl = preferences[BaseUrl] ?: return@map null
        val token = preferences[AccessToken] ?: return@map null
        AuthSession(
            baseUrl = baseUrl,
            accessToken = token,
            userName = preferences[UserName].orEmpty(),
            userEmail = preferences[UserEmail].orEmpty(),
            userRole = preferences[UserRole].orEmpty()
        )
    }

    suspend fun save(session: AuthSession) {
        context.sessionDataStore.edit { preferences ->
            preferences[BaseUrl] = session.baseUrl
            preferences[AccessToken] = session.accessToken
            preferences[UserName] = session.userName
            preferences[UserEmail] = session.userEmail
            preferences[UserRole] = session.userRole
        }
    }

    suspend fun clear() {
        context.sessionDataStore.edit { it.clear() }
    }

    private companion object {
        val BaseUrl = stringPreferencesKey("base_url")
        val AccessToken = stringPreferencesKey("access_token")
        val UserName = stringPreferencesKey("user_name")
        val UserEmail = stringPreferencesKey("user_email")
        val UserRole = stringPreferencesKey("user_role")
    }
}
