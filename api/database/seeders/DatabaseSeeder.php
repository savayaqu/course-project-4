<?php

namespace Database\Seeders;

use App\Models\ComplaintType;
use App\Models\Role;
use App\Models\User;
// use Illuminate\Database\Console\Seeds\WithoutModelEvents;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Str;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        $roleAdminId = Role::firstOrCreate(['code' => 'admin']);
        $roleUserId  = Role::firstOrCreate(['code' => 'user' ])->id;

        $genPass = Str::password(16);
        Log::info("Password for admin: $genPass");
        User::create([
            'name'       => 'Administrator',
            'login'      => 'admin',
            'password'   => $genPass,
            'role_id'    => $roleAdminId,
        ]);

        ComplaintType::firstOrCreate(['name' => 'Детская порнография']);
        ComplaintType::firstOrCreate(['name' => 'Расчленёнка']);
    }
}
