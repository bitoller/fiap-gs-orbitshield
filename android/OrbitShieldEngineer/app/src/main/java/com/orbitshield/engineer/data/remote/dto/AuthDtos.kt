package com.orbitshield.engineer.data.remote.dto

data class LoginRequestDto(
    val email: String,
    val password: String
)

data class AuthResponseDto(
    val accessToken: String,
    val tokenType: String,
    val expiresAt: String,
    val user: UserProfileDto
)

data class UserProfileDto(
    val id: Int,
    val name: String,
    val email: String,
    val role: String
)
