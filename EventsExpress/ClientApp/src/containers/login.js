﻿import { connect } from 'react-redux';
import React, {Component} from 'react';
import Login  from '../components/login';
import login from '../actions/login';


class LoginWrapper extends Component {
  submit = values => {
    this.props.login(values.email, values.password);
  };
  render() {

    let { isLoginPending, isLoginSuccess, loginError } = this.props;
    
    return <div>
              <Login onSubmit={this.submit} />
              {loginError && 
              <p className="text-danger text-center">{loginError}</p>
              }
           </div>
    ;
  }
}
const mapStateToProps = state => {
    return state.login;
};

const mapDispatchToProps = dispatch => {
  return {
    login: (email, password) => dispatch(login(email, password))
  };
};
export default connect(
  mapStateToProps,
  mapDispatchToProps
)(LoginWrapper);