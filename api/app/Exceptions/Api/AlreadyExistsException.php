<?php

namespace App\Exceptions\Api;

class AlreadyExistsException extends ApiException
{
    public function __construct($model)
    {
        $name = class_basename($model);
        $message = ucfirst(($model ?  "$name " : '') . 'already exists');
        parent::__construct($message, 409, data: [$name => $model]);
    }
}
