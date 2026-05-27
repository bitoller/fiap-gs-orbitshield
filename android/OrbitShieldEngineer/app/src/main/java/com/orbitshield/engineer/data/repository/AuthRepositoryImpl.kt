package com.orbitshield.engineer.data.repository

import com.orbitshield.engineer.data.mapper.toDomain
import com.orbitshield.engineer.data.remote.RetrofitProvider
import com.orbitshield.engineer.data.remote.dto.LoginRequestDto
import com.orbitshield.engineer.domain.model.AuthSession
import com.orbitshield.engineer.domain.repository.AuthRepository

class AuthRepositoryImpl : AuthRepository {
    override suspend fun login(baseUrl: String, email: String, password: String): AuthSession {
        val api = RetrofitProvider.create(baseUrl)
        return api.login(LoginRequestDto(email = email.trim(), password = password)).toDomain(baseUrl.trim())
    }
}
