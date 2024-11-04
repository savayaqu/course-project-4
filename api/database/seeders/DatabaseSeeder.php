<?php

namespace Database\Seeders;

use App\Models\ComplaintType;
use App\Models\Role;
use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Storage;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database.
     */
    public function run(): void
    {
        Cache::flush();
        Storage::deleteDirectory('users');
       $role_admin = Role::create([
           'code' => 'admin',
       ]);
       Role::create([
           'code' => 'user',
       ]);

       User::create([
           'name'       => 'Администратор',
           'login'      => 'admin'        ,
           'password'   => 'Admin123!'    ,
           'role_id'    => $role_admin->id,
       ]);
       ComplaintType::create([
          'name' => 'Детская порнография'
       ]);
        ComplaintType::create([
            'name' => 'Расчленёнка'
        ]);

    }
}
