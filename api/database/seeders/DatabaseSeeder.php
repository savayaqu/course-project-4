<?php

namespace Database\Seeders;

use App\Models\Role;
use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
       $role_admin = Role::create([
           'code' => 'admin',
       ]);
       Role::create([
           'code' => 'user',
       ]);

       User::create([
           'name'       => 'admin',
           'login'      => 'admin'         ,
           'password'   => 'admin'         ,
           'role_id'    => $role_admin->id,
       ]);
    }
}
