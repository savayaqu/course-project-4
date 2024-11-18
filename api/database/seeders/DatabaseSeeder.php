<?php

namespace Database\Seeders;

use App\Models\ComplaintType;
use App\Models\Role;
use App\Models\User;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Log;
use Illuminate\Support\Str;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        $roleAdminId = Role::firstOrCreate(['code' => 'admin']);
        $roleAdminId = Role::firstOrCreate(['code' => 'admin'])->id;
        $roleUserId  = Role::firstOrCreate(['code' => 'user' ])->id;
        $this->command->option('Do you wish to continue?', true);
        $admin = User
            ::where('login', 'admin')
            ->first();
        if ($admin) {
            $changePass = $this->command->confirm('Do you want change password for admin?', true);
            if ($changePass) $admin->update([
                'password' => $this->askOrGenPassword(),
                'role_id' => $roleAdminId
            ]);
        }
        else User::create([
            'name'       => 'Administrator',
            'login'      => 'admin',
            'password'   => $this->askOrGenPassword(),
            'role_id'    => $roleAdminId,
        ]);

        ComplaintType::firstOrCreate(['name' => 'Детская порнография']);
        ComplaintType::firstOrCreate(['name' => 'Расчленёнка']);
    }

    private function askOrGenPassword(): string
    {
        $password = $this->command->secret('Enter the admin password (leave blank to generate a random password)');

        if (empty($password)) {
            $password = Str::password(16);

            $choice = $this->command->choice(
                'How do you want to handle the generated password?',
                ['Output to screen', 'Save to log', 'Both'],
                0
            );

            if ($choice === 'Save to log' || $choice === 'Both')
                Log::info("Admin user created with password: $password");

            if ($choice === 'Output to screen' || $choice === 'Both')
                $this->command->info("Admin password: $password");
        }
        return $password;
    }
}
