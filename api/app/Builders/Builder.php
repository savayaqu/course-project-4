<?php

namespace App\Builders;

use App\Exceptions\Api\NotFoundException;
use Illuminate\Database\Eloquent\Builder as EloquentBuilder;
use Illuminate\Database\Eloquent\ModelNotFoundException;

class Builder extends EloquentBuilder
{
    public function firstOrFailCustom(...$args)
    {
        try {
            return parent::firstOrFail(...$args);
        } catch (ModelNotFoundException) {
            throw new NotFoundException(get_class($this->model));
        }
    }
}
