import React, {Component} from 'react';
import EventForm from '../components/event/event-form';
import { edit_event } from '../actions/add-event';
import get_countries from '../actions/countries';
import { connect } from 'react-redux';
import {getFormValues, reset, formValues} from 'redux-form';
import get_cities from '../actions/cities';
import { setEventError, setEventPending, setEventSuccess } from '../actions/add-event';
import { resetEvent } from '../actions/event-item-view';
import get_categories from '../actions/category-list';

class EditEventWrapper extends Component{
    
    componentWillMount = () =>{
        this.props.get_countries();
        this.props.get_categories();
        this.props.get_cities(this.props.initialValues.countryId);
    }
    componentDidUpdate = () => {
        if(!this.props.add_event_status.errorEvent && this.props.add_event_status.isEventSuccess){
            this.props.reset();
        }
    }

    componentWillUnmount(){
        this.props.reset();
    }

    onSubmit = (values) => {
        this.props.add_event({ ...values, user_id: this.props.user_id, id: this.props.initialValues.id });
    }

    onChangeCountry = (e) => {
        this.props.get_cities(e.target.value);
    }

    render(){   
        return <>
                <EventForm data={this.props.initialValues} 
                all_categories={this.props.all_categories} 
                cities={this.props.cities.data} 
                onChangeCountry={this.onChangeCountry} 
                onSubmit={this.onSubmit} 
                countries={this.props.countries.data} 
                form_values={this.props.form_values}
                Event={{}}
                isCreated={true} />
               </>
    }
}

const mapStateToProps = (state) => ({user_id: state.user.id,
     add_event_status: state.add_event, 
     countries: state.countries,
     cities: state.cities,
     all_categories: state.categories,
     initialValues: state.event.data,
     form_values: getFormValues('event-form')(state)});

const mapDispatchToProps = (dispatch) => { 
    return {
        add_event: (data) => dispatch(edit_event(data)),
        get_countries: () => dispatch(get_countries()),
        get_cities: (country) => dispatch(get_cities(country)),
        get_categories: () => dispatch(get_categories()),
        resetEvent: () => dispatch(resetEvent()),
        reset: () => {
            dispatch(reset('event-form'));
            dispatch(setEventPending(true));
            dispatch(setEventSuccess(false));
            dispatch(setEventError(null));
        }
    } 
};

export default connect(mapStateToProps, mapDispatchToProps)(EditEventWrapper);