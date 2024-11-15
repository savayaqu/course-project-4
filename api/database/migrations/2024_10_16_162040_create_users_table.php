<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('users', function (Blueprint $table) {
            $table->id();
            $table->string   ('name');
            $table->string   ('login', 64)->unique();
            $table->string   ('password');
            $table->boolean  ('is_banned')->default(false);
            $table->foreignId('role_id')->constrained();
            $table->rememberToken()->unique();
            $table->timestamps();
        });

        Schema::create('password_reset_tokens', function (Blueprint $table) {
            $table->string   ('login')->primary();
            $table->string   ('token');
            $table->timestamp('created_at')->nullable();
        });

        Schema::create('sessions', function (Blueprint $table) {
            $table->string   ('id')->primary();
            $table->foreignId('user_id')->nullable()->constrained('users')->cascadeOnDelete();
            $table->string   ('ip_address', 45)->nullable();
            $table->text     ('user_agent')->nullable();
            $table->longText ('payload');
            $table->integer  ('last_activity')->index();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('users');
        Schema::dropIfExists('password_reset_tokens');
        Schema::dropIfExists('sessions');
    }
};
