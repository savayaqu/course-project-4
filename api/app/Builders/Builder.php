<?php

namespace App\Builders;

use App\Exceptions\Api\NotFoundException;
use Illuminate\Database\Eloquent\Builder as EloquentBuilder;

class Builder extends EloquentBuilder
{
    public function firstOrFailCustom(...$args)
    {
        try {
            return $this->firstOrFail(...$args);
        } catch (\Exception $e) {
            throw new NotFoundException(get_class($this->model));
        }
    }
}
