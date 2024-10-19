<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('pictures', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->string('path');
            $table->string('hash');
            $table->string('preview')->unique()->nullable();
            $table->dateTime('date');
            $table->unique(['user_id', 'name']);
            $table->unique(['user_id', 'path']);
            $table->unique(['user_id', 'id']);
            $table->string('size');
            $table->string('width');
            $table->string('height');
            $table->foreignId('user_id')->constrained('users');
            $table->foreignId('album_id')->constrained('albums')->ondelete('cascade');
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('pictures');
    }
};
