<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('pictures', function (Blueprint $table) {
            $table->id();
            $table->string    ('name');
            $table->string    ('hash');
            $table->dateTime  ('date');
            $table->bigInteger('size');
            $table->integer   ('width');
            $table->integer   ('height');
            $table->foreignId ('album_id')->constrained()->cascadeOnDelete();
            $table->timestamps();

            $table->unique(['album_id', 'name']);
            $table->unique(['album_id', 'hash']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('pictures');
    }
};
