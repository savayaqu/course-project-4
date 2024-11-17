<?php

namespace Database\Factories;

use App\Models\Role;
use Illuminate\Database\Eloquent\Factories\Factory;

class UserFactory extends Factory
{
    protected static ?string $password;

    public function definition(): array
    {
        return [
            'name'  => fake()->name(),
            'login' => fake()->username(),
            'password' => static::$password ??= 'Pass123!',
            'role_id' => Role::findOrCreate(['code' => 'user'])->id,
        ];
    }
}
