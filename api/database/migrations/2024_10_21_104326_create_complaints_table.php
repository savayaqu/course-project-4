<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('complaints', function (Blueprint $table) {
            $table->id();
            $table->string   ('description')->nullable();
            $table->boolean  ('status')->nullable()->default(null);
            $table->foreignId('album_id'  )->nullable()->constrained()->nullOnDelete();
            $table->foreignId('picture_id')->nullable()->constrained()->nullOnDelete();
            $table->foreignId('from_user_id' )->constrained('users')->cascadeOnDelete();
            $table->foreignId('about_user_id')->constrained('users')->cascadeOnDelete();
            $table->foreignId('complaint_type_id')->constrained('complaint_types');
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('complaints');
    }
};
